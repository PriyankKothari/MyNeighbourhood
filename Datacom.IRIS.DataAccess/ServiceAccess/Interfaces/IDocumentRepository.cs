using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DomainModel.DTO;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IDocumentRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactSubLink> GetContactsSublinksByIRISObjectID(long irisObjectID,
                                                                bool excludeDeceasedContact,
                                                                ContactSelectionSendViaOption? sendViaOption,
                                                                List<CDFQuestionAnswerSearchCriterion> contactGroupCDFSearchCriteria);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactSubLink> GetContactsSublinksByPredefinedQuery(long predefinedQueryID, long irisObjectID);

        [DoNotGenerateBusinessWrapperAttribute]
        List<ContactSubLink> GetContactsSublinksByIDs(List<long> contactSublinkIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactQuery> GetContactQueries(long objectTypeID, long irisObjectID);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactQuery> GetContactQueries(long objectTypeID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactQuery> GetAllContactQueries();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<DocumentTemplate> GetDocumentTemplatesForObject(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReportsAndTemplates, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        DocumentTemplate GetDocumentTemplateByID(long templateID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReportsAndTemplates, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<DocumentTemplate> GetAllDocumentTemplates();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<MobileDocumentReferenceCriteria> GetMobileDocumentReferenceCriterion(long irisObjectId, bool isUserReferenced);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        MobileDocumentReferenceCriteria DeleteMobileDocumentReferenceCriteria(long irisObjectId, string documentID, bool isUserReferenced);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        MobileDocumentReferenceCriteria AddMobileDocumentReferenceCriteria(long irisObjectId, string documentID, bool isUserReferenced);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<DirectDocumentReferenceCriteria> GetDirectDocumentReferenceCriterionByIRISObjectID(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        DirectDocumentReferenceCriteria GetDirectDocumentReferenceCriteriaByID(long irisObjectId, long directDocumentReferenceCriteriaId);

        bool IsDocumentIDUniqueForIRISObject(string documentID, long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReportsAndTemplates, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Report GetReportByID(long reportID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Report GetInstanceReport(long irisObjectID, long reportID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGlobalReport")]
        Report GetGlobalReportByID(long reportID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReportsAndTemplates, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Report> GetAllReports();

        [DoNotGenerateBusinessWrapperAttribute]
        long GetNextDocumentReferenceNumber();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Report> GetReportsForObject(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGlobalReports")]
        List<Report> GetGlobalReports();
    }
}