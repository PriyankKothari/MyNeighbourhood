using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.Domain.Response;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class RegisterRepository : RepositoryStore, IRegisterRepository
    {
        public DamRegister GetDamRegisterByID(long damRegisterId)
        {
            var result = Context.DamRegisters
                               .Include(a => a.ExemptREF)
                               .Include(a => a.HeritageREF)
                               .Include(a => a.PrimaryUseREF)
                               .Include(a => a.SecondaryUseREF)
                               .Include(a => a.PotentialImpactClassificationREF)
                               .Include(a => a.PrimaryDamMaterialREF)
                               .Include(a => a.SecondaryDamMaterialREF)
                               .Include(a => a.SpillwayMaterialREF)
                               .Include(a => a.IRISObject.Statuses)
                               .Include("IRISObject.Statuses.StatusREF")
                               .SingleOrDefault(req => req.ID == damRegisterId);

            if (result == null) return null;

            result.IRISObject = (from irisObject in Context.IRISObject
                                     .Where(x => x.ID == result.IRISObjectID)
                                 from otherIdentifier in Context.OtherIdentifiers
                                     .Where(x => x.IRISObjectID == irisObject.ID && !x.IsDeleted).DefaultIfEmpty()
                                 from identifierContextREF in Context.ReferenceDataValue
                                     .Where(x => x.ID == otherIdentifier.IdentifierContextID).DefaultIfEmpty()
                                 from objectTypeREF in Context.ReferenceDataValue
                                     .Where(x => x.ID == irisObject.ObjectTypeID)
                                 select new { irisObject, otherIdentifier, identifierContextREF, objectTypeREF })
                                 .AsEnumerable().Select(x => x.irisObject).Distinct().Single();

            return result.TrackAll();
        }

        /// <summary>
        ///    This method returns 'light' DamRegister objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about a DamRegister such as the IRIS ID and DamRegister Name
        /// </summary>
        /// <param name="irisObjectIDList"></param>
        /// <returns></returns>
        public List<DamRegister> GetDamRegisterByIRISObjectIDs(List<long> irisObjectIDList)
        {
            var requestQuery = from req in Context.DamRegisters
                                   .Include(a => a.IRISObject)
                                   .Include(a => a.IRISObject.ObjectTypeREF)
                                   .Where(req => irisObjectIDList.Contains(req.IRISObjectID))
                               select req;

            return requestQuery.ToList();
        }

        public GeneralRegister GetGeneralRegisterByIrisObjectId(long irisObjectId)
        {
            long registerId = Context.GeneralRegisters.FirstOrDefault(x => x.IRISObjectID == irisObjectId).ID;
            return GetGeneralRegisterByID(registerId);
        }

        public GeneralRegister GetGeneralRegisterByID(long generalRegisterID)
        {
            var result = Context.GeneralRegisters
                               .Include(a => a.OfficerResponsible)
                               .Include(a => a.IRISObject.Statuses)
                               .Include("IRISObject.Statuses.StatusREF")
                               .Include("IRISObject.ActivityObjectRelationships.ContactSubLink.Contact")
                               .Include("IRISObject.ActivityObjectRelationships.ActivityObjectRelationshipType")
                               .SingleOrDefault(req => req.ID == generalRegisterID);

            if (result == null) return null;

            result.IRISObject = (from irisObject in Context.IRISObject
                                     .Where(x => x.ID == result.IRISObjectID)
                                 from otherIdentifier in Context.OtherIdentifiers
                                     .Where(x => x.IRISObjectID == irisObject.ID && !x.IsDeleted).DefaultIfEmpty()
                                 from identifierContextREF in Context.ReferenceDataValue
                                     .Where(x => x.ID == otherIdentifier.IdentifierContextID).DefaultIfEmpty()
                                 from objectTypeREF in Context.ReferenceDataValue
                                     .Where(x => x.ID == irisObject.ObjectTypeID)
                                 from subClass1REF in Context.ReferenceDataValue
                                     .Where(x => x.ID == irisObject.SubClass1ID)
                                 select new { irisObject, otherIdentifier, identifierContextREF, objectTypeREF, subClass1REF })
                                 .AsEnumerable().Select(x => x.irisObject).Distinct().Single();

            return result.TrackAll();
        }

        /// <summary>
        ///    This method returns 'light' generalRegister objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about a generalRegister such as the IRIS ID and generalRegister Name and Type
        /// </summary>
        /// <param name="irisObjectIDList"></param>
        /// <returns></returns>
        public List<GeneralRegister> GetGeneralRegisterByIRISObjectIDs(List<long> irisObjectIDList)
        {
            var requestQuery = from req in Context.GeneralRegisters
                                   .Include(a => a.IRISObject)
                                   .Include(a => a.IRISObject.ObjectTypeREF)
                                   .Include(a => a.IRISObject.SubClass1REF)
                                   .Where(req => irisObjectIDList.Contains(req.IRISObjectID))
                               select req;

            return requestQuery.ToList();
        }

        public ServiceResponse<GeneralRegister> PopulateRegisterRelationship(GeneralRegister register, string activityRelationshipTypeCode, Contact contact)
        {
            ServiceResponse<GeneralRegister> res = new ServiceResponse<GeneralRegister>();
            res.Try(() =>
            {
                ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
                               
            });
            return res;
        }

        public List<GeneralRegister> GetAllGeneralRegisters()
        {
            var query = Context.GeneralRegisters
                .Include(a => a.IRISObject)
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Include(a => a.IRISObject.SubClass1REF);
            return query.ToList();
        }
    }
}
