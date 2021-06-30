using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class LocationRepository : RepositoryStore, ILocationRepository
    {       
        public LocationGroup GetLocationGroupByID(long id)
        {
            return Context.LocationGroup
                    .Include(l => l.IRISObject)
                    .Include(l => l.IRISObject.ObjectTypeREF)
                    .Include(l => l.StatusREF)
                    .Include(l => l.Owner)
                    .Where(l => l.ID == id)
                    .Single().TrackAll();
        }

        public List<LocationGroup> GetAllLocationGroups()
        {
            return Context.LocationGroup.Include(l => l.IRISObject.ObjectTypeREF).ToList();
        }

        public Location GetLocationByID(long id)
        {
            Location location = Context.Location
                                    .Include(l => l.IRISObject)
                                    .Include(l => l.IRISObject.ObjectTypeREF)
                                    .Include(l => l.IRISObject.SubClass1REF)
                                    .Include(l => l.IRISObject.SubClass2REF)
                                    .Include(l => l.ReliabilityREF)
                                    .Single(l => l.ID == id).TrackAll();

            location.IRISObject.OtherIdentifiers.AddRange(
                Context.OtherIdentifiers
                    .Include(oi => oi.IdentifierContextREF)
                    .Where(oi => oi.IRISObjectID == location.IRISObjectID && !oi.IsDeleted)
                    .ToList()
            );

            if (location.IsTextual)
            {
                TextualLocation textualLocation = (TextualLocation) location;
                textualLocation.Address = Context.Address.Single(tl => tl.ID == textualLocation.AddressID).TrackAll();
            }
            else
            {
                SpatialLocation spatialLocation = (SpatialLocation)location;
                spatialLocation.DatumREF = Context.ReferenceDataValue.SingleOrDefault(tl => tl.ID == spatialLocation.DatumID);
                spatialLocation.CaptureMethodREF = Context.ReferenceDataValue.Single(tl => tl.ID == spatialLocation.CaptureMethodID);
            }

            return location;
        }

        public List<Location> GetSpatialLocationsByIrisObjectIds(params long[] irisObjectIdValues)
        {
            return Context.Location
                    .Include(l => l.IRISObject)
                    .Include(l => l.IRISObject.ObjectTypeREF)
                    .Include(l => l.IRISObject.SubClass1REF)
                    .Include(l => l.IRISObject.SubClass2REF)
                    .Include(l => l.IRISObject.OtherIdentifiers)
                    .Include(l => l.ReliabilityREF)
                    .WhereIn(i => i.IRISObjectID, irisObjectIdValues)
                    .ToList().TrackAll();
        }

        public TextualLocation GetFirstTextualLocationByIrisObjectId(List<long> irisObjectIDs)
        {
            TextualLocation textualLocation = null;

            Location location = Context.Location
                    .Include(l => l.IRISObject)
                    .Include(l => l.IRISObject.ObjectTypeREF)
                    .Where(i => irisObjectIDs.Contains(i.IRISObjectID) && i.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.LocationType.Textual)
                    .OrderBy(l => l.DateCreated)
                    .FirstOrDefault();

            if (location != null)
            {
                textualLocation = (TextualLocation)location;
                textualLocation.Address = Context.Address.Single(tl => tl.ID == textualLocation.AddressID).TrackAll();
            }

            return textualLocation;
        }

        /// <summary>
        ///    A location group must always have a unique name. Hence this method performs check
        /// </summary>
        public bool IsLocationGroupCommonNameUnique(LocationGroup locationGroup)
        {
            return Context.LocationGroup.Where(lg => string.Compare(lg.CommonName, locationGroup.CommonName, true) == 0 && 
                                                        lg.ID != locationGroup.ID
                   ).Count() == 0;
        }

        /// <summary>
        ///    This method is used to support the linking grids. Given a list of iris object
        ///    Id's (what is being rendered in a grid), this method will return back a 
        ///    dictionary mapping between ID and warning comments to be used for rendering
        ///    at the front end.
        /// </summary>
        /// <param name="irisObjectIdList"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetLocationWarnings(List<long> irisObjectIdList)
        {
            return Context.Location
                        .Where(c => !string.IsNullOrEmpty(c.WarningComments))        
                        .Where(c => irisObjectIdList.Contains(c.IRISObjectID))
                        .Select(c => new { c.IRISObjectID, c.WarningComments })
                        .ToDictionary(c => c.IRISObjectID, c => c.WarningComments);
        }

        public Dictionary<long, string> GetLocationGroupWarnings(List<long> irisObjectIdList)
        {
            return Context.LocationGroup
                        .Where(c => !string.IsNullOrEmpty(c.WarningComments))
                        .Where(c => irisObjectIdList.Contains(c.IRISObjectID))
                        .Select(c => new { c.IRISObjectID, c.WarningComments })
                        .ToDictionary(c => c.IRISObjectID, c => c.WarningComments);
        }

        public string HasAssociateGeometry(long irisObjectID)
        {
            ObjectParameter hasAssociateGeometry = new ObjectParameter("HasAssociateGeometry", typeof(string));
            Context.HasAssociateGeometry(irisObjectID, hasAssociateGeometry);
            return hasAssociateGeometry.Value != null && !Convert.IsDBNull(hasAssociateGeometry.Value) ? ((string)hasAssociateGeometry.Value).ToLower() : "0";
        }


        public TextualLocation GetFirstTextualLocationLinkedtoApplicationActivities(long applicationIRISObjectID)
        {
            var queryDirection1 = from app in Context.Applications
                                  join activity in Context.Activities on app.ID equals activity.ApplicationID
                                  join relationship in Context.ActivityObjectRelationship on activity.IRISObjectID equals relationship.IRISObjectID
                                  join linkedIRISObject in Context.IRISObject on relationship.RelatedIRISObjectID equals linkedIRISObject.ID
                                  where relationship.CurrentTo == null && app.IRISObjectID == applicationIRISObjectID
                                  select linkedIRISObject;

            var queryDirection2 = from app in Context.Applications
                                  join activity in Context.Activities on app.ID equals activity.ApplicationID
                                  join relationship in Context.ActivityObjectRelationship on activity.IRISObjectID equals relationship.RelatedIRISObjectID
                                  join linkedIRISObject in Context.IRISObject on relationship.IRISObjectID equals linkedIRISObject.ID
                                  where relationship.CurrentTo == null && app.IRISObjectID == applicationIRISObjectID
                                  select linkedIRISObject;

            var firstTextualLocation = (from locationIRISObject in queryDirection1.Concat(queryDirection2)
                                        join location in Context.Location.OfType<TextualLocation>() on locationIRISObject.ID equals location.IRISObjectID
                                        orderby location.DateCreated
                                        select new
                                        {
                                            TextualLocation = location,
                                            location.IRISObject,
                                            location.IRISObject.ObjectTypeREF,
                                            location.IRISObject.SubClass1REF,
                                            location.Address
                                        }).FirstOrDefault();

            if (firstTextualLocation != null)
                return firstTextualLocation.TextualLocation;
            else
                return null;
        }
    }
}
