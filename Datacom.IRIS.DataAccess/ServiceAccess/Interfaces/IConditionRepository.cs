using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IConditionRepository : IRepositoryBase
    {

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //LibaryCondition is like reference data
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<LibraryCondition> GetAllLibraryConditions(bool includeInactive);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //LibaryCondition is like reference data
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        LibraryCondition GetLibraryConditionByID(long libraryConditionID);

        [DoNotGenerateBusinessWrapperAttribute]
        List<Condition> GetAllConditionsForAuthorisation(long authorisationID, bool currentOnly);  //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        List<Condition> GetAllConditionsWithinAuthorisation(long conditionID, bool currentOnly);  //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        List<Condition> GetAllConditionsForActivity(long activityID, bool currentOnly); //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        List<Condition> GetAllConditionsWithinActivity(long conditionID, bool currentOnly); //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        List<Condition> GetAllConditionsWithinConditionSchedule(long conditionID, bool currentOnly); //only used by the business layer
        
        [DoNotGenerateBusinessWrapperAttribute]
        bool IsConditionLibraryInUse(long libraryConditionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke="FilterAccessToConditions")]
        Condition GetConditionByID(long conditionID);


        [DoNotGenerateBusinessWrapperAttribute]
        IRISObject GetIRISObjectByConditionScheduleID(long conditionScheduleID);


        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        ConditionSchedule GetConditionScheduleByID(long conditionScheduleId);
    }
}