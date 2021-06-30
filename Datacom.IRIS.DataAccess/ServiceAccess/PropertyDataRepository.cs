using Datacom.IRIS.DataAccess.PropertyData;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.IO;
using System.Linq;
using Unity;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class PropertyDataRepository : RepositoryStore, IPropertyDataRepository
    {
        public IEnumerable<PropertyDataDVRUpload> GetDVRUploads()
        {
            return Context.PropertyDataDVRUploads
                            .Include(x => x.User)
                            .Include(x => x.DVRUploadStatusREF)
                            .Where(x => !x.IsDeleted).ToList();
        }

        public PropertyDataDVRUpload GetDVRUploadById(long id)
        {
            return Context.PropertyDataDVRUploads.Single(x => x.ID == id && !x.IsDeleted).TrackAll();
        }

        public PropertyDataDVRUpload UploadDVRFile(long propertyDataDVRUploadID, string filePath, bool isFullRefresh)
        {
            var startDT = DateTime.Now;

            //Upload records
            var result = Context.PropertyDataDVRUploads.Single(x => x.ID == propertyDataDVRUploadID);
            Context.PropertyDataImportReset(); 
            var uploader = new DVRFileReader(File.OpenText(filePath), propertyDataDVRUploadID, new DVRFileSQLBulkUploader());
            const int defaultThrottle = 2000;
            var throttle = defaultThrottle;
            var throttleAppSetting = Context.AppSetting.SingleOrDefault(a => a.Key == AppSettingConstants.PropertyDataUploadThrottle);
            if (throttleAppSetting != null)
            {
                if (!int.TryParse(throttleAppSetting.Value, out throttle)) throttle = defaultThrottle;
            }
            uploader.ThrottleUpdateLimit = throttle;
            uploader.Read();
            
            //Update status to 'Validating'.           
            var collectionID = Context.ReferenceDataCollection.Single(x => x.Code == "PropertyDataDVRImportStatuses").ID;
            result.DVRUploadStatusREFID = Context.ReferenceDataValue.Single(x => x.Code == "Validating" && x.CollectionID == collectionID).ID;
            ApplyEntityChanges(result);

            //Notify upload is complete.
            Context.PropertyDataImportMessages.AddObject(new PropertyDataImportMessage
            {
                StartDateTime = startDT,
                EndDateTime = DateTime.Now,
                DVRUploadStatusREFID = result.DVRUploadStatusREFID,
                PropertyDataDVRUploadID = result.ID,
                Message = "File uploaded."
            });

            //Add processing message.
            //
            //Note: We must create this message outside of the Aysnc call.
            //      The the Aysnc thread does not have access to SecurityHelper.CurrentUserName 
            //      and this causes the IRISContext..SetAuditingProperties method to fail.
            //      We can update an entity because this does not call the audit method.
            var importMessage = new PropertyDataImportMessage
            {
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
                DVRUploadStatusREFID = result.DVRUploadStatusREFID,
                PropertyDataDVRUploadID = result.ID,
                Message = "File loading..."
            };
            Context.PropertyDataImportMessages.AddObject(importMessage);

            //Write changes to DB.
            SaveChanges();

            //Run stored procedure to process uploaded records asynchronously.
            var asyncAction = new Action(() =>
            {
                var context = Common.DependencyInjection.DIContainer.Container.Resolve<IObjectContext>();
                try
                {
                    //Set connection timeout.
                    var appSetting = context.AppSetting.SingleOrDefault(a => a.Key == AppSettingConstants.PropertyDataUploadConnectionTimeout);
                    const int defaultTimeOutValue = 600;
                    var connectionTimeOutValue = defaultTimeOutValue;
                    if (appSetting != null)
                    {
                        if (!int.TryParse(appSetting.Value, out connectionTimeOutValue)) connectionTimeOutValue = defaultTimeOutValue;
                    }
                    
                    var objectContext = context as ObjectContext;
                    
                    if (objectContext != null) objectContext.CommandTimeout = connectionTimeOutValue;

                    //Process upload
                    context.PropertyDataImport(propertyDataDVRUploadID);
                    importMessage.Message = "File load completed.";
                }
                catch (Exception ex)
                {
                    // Set status to 'Error'
                    var errorStatusREFID = Context.ReferenceDataValue.Single(x => x.Code == "Error" && x.CollectionID == collectionID).ID;
                    result.DVRUploadStatusREFID = errorStatusREFID;
                    ApplyEntityChanges(result);

                    // Log exception
                    importMessage.Message = string.Format("File load error in dbo.PropertyDataImport({2}) : {0}, InnerException: {1}", ex.Message, ex.InnerException == null ? "No inner exception." : ex.InnerException.Message, propertyDataDVRUploadID);
                }
                finally
                {
                    // Log end timings.
                    importMessage.EndDateTime = DateTime.Now;
                    importMessage.LastModified = importMessage.EndDateTime;
                    ApplyEntityChanges(importMessage);
                    SaveChanges();
                    if (context != null) context.Dispose();                    
                }
            });
            asyncAction.BeginInvoke(null,null);

            return result;
        }

        public PropertyDataValuation GetPropertyValuationById(long id)
        {
            var result = Context.PropertyDataValuations.Single(x => x.ID == id);

            result.PropertyDataOwners.AddTrackableCollection(Context.PropertyDataOwners.Where(x => x.PropertyDataValuationID == id).ToList());
            result.PropertyDataRatepayers.AddTrackableCollection(Context.PropertyDataRatepayers.Where(x => x.PropertyDataValuationID == id).ToList());

            return result;
        }

        public PropertyDataDVRUpload DeleteUploadHistoryById(long propertyDataDVRUploadID)
        {
            Context.PropertyDataImportDelete(propertyDataDVRUploadID);
            return Context.PropertyDataDVRUploads.Single(x => x.ID == propertyDataDVRUploadID);
        }
    }
}