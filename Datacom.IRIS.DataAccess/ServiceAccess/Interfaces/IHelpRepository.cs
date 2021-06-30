using System.Collections.Generic;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IHelpRepository : IRepositoryBase
    {
        #region Methods accessed by IRIS admin help section; NOT main front facing IRIS application
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Help, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        HelpLink GetHelpLinkByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Help, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<HelpContentDTO> GetAllHelpContentDTOs();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Help, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<HelpLinkDTO> GetAllHelpLinkDTOs();

        /// <summary>
        ///    This method is used internally by the Business tier and need not be exposed
        ///    to the outside world. It is used as part of the business validation steps.
        /// </summary>
        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        HelpContent GetHelpContentByNameWithDifferentID(string name, long id);

        /// <summary>
        ///    This method is used internally by the Business tier and need not be exposed
        ///    to the outside world. It is used as part of the business validation steps.
        /// </summary>
        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<HelpLink> GetExistingHelpLinksWithContentId(long contentId);

        #endregion

        /// <summary>
        ///    This method is invoked in two places in the UI. In the help admin pages and
        ///    in the Help.aspx page which renders the help content.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        HelpContent GetHelpContentByID(long id);

        // Might possibly need this method
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<HelpLink> GetHelpLinkByName(params string[] name);
    }
}