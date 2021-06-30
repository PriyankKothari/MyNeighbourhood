using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Interfaces;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    /// <summary>
    ///    This repository has been introduced to be used by Domain objects that need to
    ///    retrieves the security contexts.
    ///    Both the Security Context object and the ISecurableRepository interface are 
    ///    stored in the Common library so that they can be invoked from the IRIS Domain project.
    /// </summary>
    public class SecurableRepository : RepositoryStore, ISecurableRepository
    {
    	
        public SecurityContext GetIRISObjectSecurityContext(long irisId)
        {
            var dbquery =  from irisObject in Context.IRISObject
                           where irisObject.ID == irisId
                           select new SecurityContext
                           {
                                ObjectTypeCode = (irisObject.SecurityContextIRISObject == null)? irisObject.ObjectTypeREF.Code : irisObject.SecurityContextIRISObject.ObjectTypeREF.Code,
                                ObjectTypeID = (irisObject.SecurityContextIRISObject == null) ? irisObject.ObjectTypeREF.ID : irisObject.SecurityContextIRISObject.ObjectTypeREF.ID,
                                IRISObjectID = (irisObject.SecurityContextIRISObject == null)? irisObject.ID : irisObject.SecurityContextIRISObject.ID,
                                SubClass1ID = (irisObject.SecurityContextIRISObject == null) ? irisObject.SubClass1ID : irisObject.SecurityContextIRISObject.SubClass1ID,
                                SubClass2ID = (irisObject.SecurityContextIRISObject == null) ? irisObject.SubClass2ID : irisObject.SecurityContextIRISObject.SubClass2ID,
                                ForIRISObjectID = irisObject.ID
                           };
                       
            
            return dbquery.AsEnumerable().Single();
        }

    }
}
