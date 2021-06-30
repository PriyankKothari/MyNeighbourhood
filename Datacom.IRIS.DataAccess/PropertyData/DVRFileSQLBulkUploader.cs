using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.DTO.PropertyData;
using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Diagnostics;
using Unity;

namespace Datacom.IRIS.DataAccess.PropertyData
{
    public class DVRFileSQLBulkUploader : IBulkUploader
    {
        public DVRFileReader DVRFileReader { get; set; }

        public bool BulkUpload()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var context = DIContainer.Container.Resolve<IObjectContext>();
            var connString = (context.Connection != null && context.Connection is EntityConnection) ? (context.Connection as EntityConnection).StoreConnection.ConnectionString : "";

            using (var bulkCopy = new SqlBulkCopy(connString))
            {
                string stage = "";
                try
                {
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportARecord";
                    bulkCopy.WriteToServer(DVRFileReader.PropertyDetails.AsDataReader());
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportBRecord";
                    bulkCopy.WriteToServer(DVRFileReader.RatePayerDetails.AsDataReader());
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportCRecord";
                    bulkCopy.WriteToServer(DVRFileReader.OwnerDetails.AsDataReader());
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportDRecord";
                    bulkCopy.WriteToServer(DVRFileReader.PropertyDetailsPart2.AsDataReader());
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportERecord";
                    bulkCopy.WriteToServer(DVRFileReader.LegalDescription.AsDataReader());
                    bulkCopy.DestinationTableName = stage = "PropertyDataImportRecordCounts";
                    bulkCopy.WriteToServer(DVRFileReader.RecordCounts.AsDataReader());
                }
                catch (Exception ex)
                {
                    var errMessage = string.Format("SQL bulk copy error attempting to write {2} EDE+ file data to the database: {0}, InnerException: {1}", ex.Message, ex.InnerException == null ? "No inner exception." : ex.InnerException.Message, stage);

                    //Some validation takes place on the upload and we remove these errors when we get a bulk copy error to keep the focus on the fundamental copy error.
                    DVRFileReader.ImportErrors = new List<PropertyDataImportError>
                    {
                        new PropertyDataImportError(DVRFileReader.RecordCounts[0].PropertyDataDVRUploadID, 0, errMessage, DVRFileReader.RecordCounts[0].CreatedBy)
                    };
                }
                finally
                {
                    //Note: If we get an error writing the errors then let it be raised as its likely to be a fundamental error like server down.
                    bulkCopy.DestinationTableName = "PropertyDataImportError";
                    bulkCopy.WriteToServer(DVRFileReader.ImportErrors.AsDataReader());
                }
            }

            stopwatch.Stop();
            Duration = stopwatch.Elapsed;

            return true;
        }

        public TimeSpan? Duration { get; set; }
        public string ErrorMessage { get; set; }
    }
}