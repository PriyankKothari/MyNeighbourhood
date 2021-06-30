using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ISpeciesRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.SpeciesData, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Species GetSpeciesByID(long speciesID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.SpeciesData, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Species> GetAllSpeciesForSpeciesType(bool includeInactive, long speciesTypeID);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Species for adding/editting 
        ///  the species count in ObservationMngt. We skip the security checking since the 
        ///  Species data are used as reference data here. To use it in other places, the security 
        ///  checking need be reviewed.
        /// </summary>
        /// <param name="speciesTypeREFID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Species> GetSpeciesListBySpeciesTypeID(long speciesTypeREFID);

        //This is for preload cache data
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SpeciesType> GetAllSpeciesType();

        /// <summary>
        ///  This method is only used in BusinessValidationRules checking, so don't need security check
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        bool IsSpeciesScientificNameUnique(Species species);

        /// <summary>
        ///  This method is only used in BusinessValidationRules checking, so don't need security check
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        bool IsSpeciesCommonNameUnique(Species species);
    }
}