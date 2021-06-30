using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DomainModel.Domain.Constants;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class CDFRepository : RepositoryStore, ICDFRepository
    {
        /// <summary>
        ///    This method is typically called by Admin backend screens that require a listing of all
        ///    CDF Lists that have been created in the system. Associated Questions and Answers are 
        ///    not retrieved.
        /// </summary>
        public List<CDFList> GetCDFLists()
        {
            return Context.CDFList
                    .Include(c => c.ObjectTypeREF)
                    .Include(c => c.SubClassification1REF)
                    .Include(c => c.SubClassification2REF)
                    .Include(c => c.SubClassification3REF)
                    .Include("CDFListQuestionDefinitions.QuestionDefinition")
                    .Where(c => !c.IsDeleted)
                    .ToList();
        }

        public List<CDFList> GetTaskCDFLists()
        {
            return Context.CDFList
                    .Include(c => c.ObjectTypeREF)
                    .Include(c => c.SubClassification1REF)
                    .Include(c => c.SubClassification2REF)
                    .Include(c => c.SubClassification3REF)
                    .Include("CDFListQuestionDefinitions.QuestionDefinition")
                    .Where(c => !c.IsDeleted && c.TaskDefinitionIDs != null)                    
                    .ToList();
        }

        public QuestionDefinition GetAllCDFDropdownValues(string objectType, string symbolName, string subClass1 = null, string subClass2 = null, string subClass3 = null)
        {
            long? subClass1Id = null, subClass2Id = null, subClass3Id = null;
            var objectTypeREF = Context.ReferenceDataValue.Single(x =>
                x.Code == objectType && x.IsCurrent &&
                x.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjects);
            if (!subClass1.IsNullOrEmpty())
            {
                subClass1Id = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == subClass1 &&
                    x.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjectsSubClass1 && x.IsCurrent &&
                    x.ParentValueID == objectTypeREF.ID).ID;
                if (!subClass2.IsNullOrEmpty())
                {
                    subClass2Id = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == subClass2 &&
                        x.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjectsSubClass2 && x.IsCurrent &&
                        x.ParentValueID == subClass1Id).ID;
                    if (!subClass3.IsNullOrEmpty())
                    {
                        subClass3Id = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == subClass3 &&
                            x.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjectsSubClass3 && x.IsCurrent &&
                            x.ParentValueID == subClass2Id).ID;
                    }
                }
            }
            var cdfLists = GetCDFLists().Where(x => x.ObjectTypeREFID == objectTypeREF.ID && x.SubClassification1REFID == subClass1Id 
                && x.SubClassification2REFID == subClass2Id && x.SubClassification3REFID == subClass3Id);
            foreach (var list in cdfLists)
            {
                foreach (var listqd in list.CDFListQuestionDefinitions)
                {
                    if (listqd.QuestionDefinition.SymbolName == symbolName && listqd.QuestionDefinition.TypeCode == QuestionTypes.OptionField 
                        && listqd.QuestionDefinition.SubTypeCode == QuestionTypes.SubTypes.DropDown)
                    {
                        return listqd.QuestionDefinition;
                    }
                }
            }
            return null;
        }

        public List<CDFList> GetCDFListsWithQuestionsAndAnswers(long irisObjectID, long objectTypeID, long? subClass1ID, long? subClass2ID, long? subClass3ID)
        {
            var cdfListsQueryable = Context.CDFList
                                    .Where(c => !c.IsDeleted)
                                    .Where(c =>
                                        (c.ObjectTypeREFID == objectTypeID && c.SubClassification1REFID == null && c.SubClassification2REFID == null && c.SubClassification3REFID == null) ||
                                        (c.ObjectTypeREFID == objectTypeID && c.SubClassification1REFID == subClass1ID && c.SubClassification2REFID == null && c.SubClassification3REFID == null) ||
                                        (c.ObjectTypeREFID == objectTypeID && c.SubClassification1REFID == subClass1ID && c.SubClassification2REFID == subClass2ID && c.SubClassification3REFID == null) ||
                                        (c.ObjectTypeREFID == objectTypeID && c.SubClassification1REFID == subClass1ID && c.SubClassification2REFID == subClass2ID && c.SubClassification3REFID == subClass3ID)
                                    ).OrderBy(c => c.OrderNumber);

            var result = GetAllCDFsWithQuestionAnswers(cdfListsQueryable, irisObjectID).Distinct().ToList();
            return result;
        }

        public List<CDFList> GetCDFListDefinitions(long objectTypeID)
        {
            var cdfListQueryable = Context.CDFList
                    .Include(c => c.SecurityContext)
                    .Where(c => !c.IsDeleted)
                    .Where(c => c.ObjectTypeREFID == objectTypeID)
                    .OrderBy(c => c.OrderNumber);

            var dbquery = from cdfList in cdfListQueryable
                          //from objectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == cdfList.ObjectTypeREFID)
                          from cdfQD in Context.CDFListQuestionDefinition
                            .Where(cqd => cqd.CDFListID == cdfList.ID
                                && Context.QuestionDefinition.Any(q => q.ID == cqd.QuestionDefinitionID && !q.IsDeleted
                                && q.TypeCode == "OPTION" && q.IsAdvancedSearchable))
                            .DefaultIfEmpty()
                          from questionDef in Context.QuestionDefinition
                            .Where(q => q.ID == cdfQD.QuestionDefinitionID && !q.IsDeleted)
                            .DefaultIfEmpty()
                          orderby cdfList.OrderNumber, questionDef.OrderNumber
                          select new { cdfList, cdfQD, questionDef };

            return dbquery.AsEnumerable().Select(p => p.cdfList).Distinct().ToList();
        }

        public List<string> GetCDFListNamesWithMissingMandatoryAnswers(long irisObjectID)
        {
            return Context.GetCDFListNameWithMissingMandatoryAnswer(irisObjectID).ToList();
        }

        public CDFList GetCDFListWithQuestionsAndAnswers(long cdfListID, long irisObjectID)
        {
            IQueryable<CDFList> cdfListQueryable = Context.CDFList.Where(c => c.ID == cdfListID && !c.IsDeleted);
            return GetAllCDFsWithQuestionAnswers(cdfListQueryable, irisObjectID).Single().TrackAll();
        }

        /// <summary>
        ///    Method that returns all cdflists with questions and answers.
        /// </summary>
        private IEnumerable<CDFList> GetAllCDFsWithQuestionAnswers(IQueryable<CDFList> cdfListQueryable, long irisObjectID)
        {
            var dbquery = from cdfList in cdfListQueryable
                          from objectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == cdfList.ObjectTypeREFID)
                          from subclassification1 in Context.ReferenceDataValue.Where(r => r.ID == cdfList.SubClassification1REFID).DefaultIfEmpty()
                          from subclassification2 in Context.ReferenceDataValue.Where(r => r.ID == cdfList.SubClassification2REFID).DefaultIfEmpty()
                          from subclassification3 in Context.ReferenceDataValue.Where(r => r.ID == cdfList.SubClassification3REFID).DefaultIfEmpty()
                          from cdfQD in Context.CDFListQuestionDefinition
                            .Where(cqd => cqd.CDFListID == cdfList.ID
                                && Context.QuestionDefinition.Any(q => q.ID == cqd.QuestionDefinitionID && !q.IsDeleted))
                            .DefaultIfEmpty()
                          from questionDef in Context.QuestionDefinition
                            .Where(q => q.ID == cdfQD.QuestionDefinitionID && !q.IsDeleted)
                            .DefaultIfEmpty()
                          from parentQuestionDef in Context.QuestionDefinition
                            .Where(pq => pq.ID == questionDef.ParentQuestionId && !pq.IsDeleted)
                            .DefaultIfEmpty()
                          from taskMappedField in Context.TaskMappedFields
                            .Where(t => questionDef.TaskMappedFieldID == t.ID)
                            .DefaultIfEmpty()
                          from answer in Context.Answer
                            .Where(a => (a.QuestionID == questionDef.ID || a.QuestionID == parentQuestionDef.ID ) && !a.IsDeleted)
                            .Where(a => a.IRISObjectID == irisObjectID)
                            .DefaultIfEmpty()
                              // for task cdfs, only include QD with valid mapping
                          from irisObject in Context.IRISObject.Where(o => o.ID == irisObjectID)
                          where taskMappedField == null || (
                                                            (taskMappedField.SubClassification3REFID == null || taskMappedField.SubClassification3REFID == irisObject.SubClass3ID) &&
                                                            (taskMappedField.SubClassification2REFID == null || taskMappedField.SubClassification2REFID == irisObject.SubClass2ID) &&
                                                            (taskMappedField.SubClassification1REFID == null || taskMappedField.SubClassification1REFID == irisObject.SubClass1ID)
                                                           )
                          orderby cdfList.OrderNumber, questionDef.OrderNumber, answer.Row
                          select new { cdfList, objectTypeREF, subclassification1, subclassification2, subclassification3, cdfQD, questionDef, taskMappedField, answer, parentQuestionDef };

            var cdfLists = dbquery.AsEnumerable().Select(p => p.cdfList).Distinct().ToList();

            //populate securityContext
            var securityContext = (from irisObject in Context.IRISObject
                                   where irisObject.ID == irisObjectID
                                   select new SecurityContext
                                   {
                                       IRISObjectID = (irisObject.SecurityContextIRISObjectID.HasValue) ? irisObject.SecurityContextIRISObjectID.Value : irisObject.ID,
                                       ObjectTypeCode = (irisObject.SecurityContextIRISObjectID.HasValue) ? irisObject.SecurityContextIRISObject.ObjectTypeREF.Code : irisObject.ObjectTypeREF.Code,
                                       ObjectTypeID = (irisObject.SecurityContextIRISObjectID.HasValue) ? irisObject.SecurityContextIRISObject.ObjectTypeID : irisObject.ObjectTypeID,
                                       SubClass1ID = (irisObject.SecurityContextIRISObjectID.HasValue) ? irisObject.SecurityContextIRISObject.SubClass1ID : irisObject.SubClass1ID,
                                       SubClass2ID = (irisObject.SecurityContextIRISObjectID.HasValue) ? irisObject.SecurityContextIRISObject.SubClass2ID : irisObject.SubClass2ID,
                                       ForIRISObjectID = irisObject.ID
                                   }).AsEnumerable().Single();
            cdfLists.ForEach(l => l.SecurityContext = securityContext);

            if (cdfLists.Count(l => l.OrderNumber != 0) > 0)
            {
                cdfLists = cdfLists
                    .OrderBy(x => x.OrderNumber).ToList();
            }
            else
            {
                cdfLists = cdfLists
                    .OrderBy(x => x.SubClassification1REFID ?? 0)
                    .ThenBy(x => x.SubClassification2REFID ?? 0)
                    .ThenBy(x => x.SubClassification3REFID ?? 0)
                    .ThenBy(x => x.SubClassification3REFID ?? 0)
                    .ThenBy(x => x.ListName).ToList();
            }
            return cdfLists;
        }

        /// <summary>
        ///    This method is typically called by Admin backend screens. It will NOT include any Answer objects
        ///    inside any of the Question Definitions because an IRIS Object ID has not been passed. Use this
        ///    method typically to get an idea of what question definitions exist inside a given CDF List.
        /// </summary>
        public CDFList GetCDFList(long cdfListID)
        {
            var dbquery = from cdfList in Context.CDFList.Where(c => c.ID == cdfListID && !c.IsDeleted)
                          from objectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == cdfList.ObjectTypeREFID)
                          from cdfQD in Context.CDFListQuestionDefinition
                            .Where(cqd => cqd.CDFListID == cdfList.ID
                                && Context.QuestionDefinition.Any(q => q.ID == cqd.QuestionDefinitionID && !q.IsDeleted))
                            .DefaultIfEmpty()
                          from questionDef in Context.QuestionDefinition
                            .Where(q => q.ID == cdfQD.QuestionDefinitionID && !q.IsDeleted)
                            .DefaultIfEmpty()
                          orderby questionDef.OrderNumber
                          select new { cdfList, objectTypeREF, cdfQD, questionDef };

            return dbquery.AsEnumerable().Select(p => p.cdfList).Distinct().Single().TrackAll();
        }

        public List<TaskMappedField> GetTaskMappedFields(long cdfListID, string questionType)
        {
            // todo: filter on subclass
            var dbquery = from cdfList in Context.CDFList.Where(c => c.ID == cdfListID && !c.IsDeleted)
                          from taskField in Context.TaskMappedFields.Where(t => t.ObjectTypeREFID == cdfList.ObjectTypeREFID && t.QuestionTypeCode == questionType)
                          select new { taskField };

            return dbquery.AsEnumerable().Select(p => p.taskField).Distinct().ToList();
        }

        public List<CDFListQuestionDefinition> GetTaskFormCDFList(long objectTypeREFID, string questionType, bool hasDecimal)
        {
            //if (hasDecimal)
            //{
            //    var dbquery = from cdfList in Context.CDFList.Where(c => c.ObjectTypeREFID == objectTypeREFID && !c.IsDeleted && c.SubClassification1REF != null && c.Type == CDFListType.FormFields)
            //                  from questionDef in Context.QuestionDefinition.Where(x =>x.TypeCode == questionType && !x.IsDeleted && 
            //                  (x.DecimalPlaces > 0 || x.DecimalPlaces == null))
            //                  from cdfQD in Context.CDFListQuestionDefinition.Where(a => a.CDFListID == cdfList.ID && a.QuestionDefinitionID == questionDef.ID)
            //                  select new { cdfQD };
            //    return dbquery.AsEnumerable().Select(p => p.cdfQD).Distinct().ToList();
            //}
            //else
            //{
            //    var dbquery = from cdfList in Context.CDFList.Where(c => c.ObjectTypeREFID == objectTypeREFID && !c.IsDeleted && c.SubClassification1REF != null && c.Type == CDFListType.FormFields)
            //                  from questionDef in Context.QuestionDefinition.Where(x => x.TypeCode == questionType && !x.IsDeleted &&
            //                  (x.DecimalPlaces == 0 || x.DecimalPlaces == null))
            //                  from cdfQD in Context.CDFListQuestionDefinition.Where(a => a.CDFListID == cdfList.ID && a.QuestionDefinitionID == questionDef.ID)
            //        select new { cdfQD };
            //    return dbquery.AsEnumerable().Select(p => p.cdfQD).Distinct().ToList();
            //}


            if (hasDecimal)
            {
                var dbquery = from cdfList in Context.CDFList.Where(c => c.ObjectTypeREFID == objectTypeREFID && !c.IsDeleted && c.TaskDefinitionIDs == null && c.Type == CDFListType.FormFields)
                              from questionDef in Context.QuestionDefinition.Where(x => x.TypeCode == questionType && !x.IsDeleted &&
                              (x.DecimalPlaces > 0 || x.DecimalPlaces == null))
                              from cdfQD in Context.CDFListQuestionDefinition.Where(a => a.CDFListID == cdfList.ID && a.QuestionDefinitionID == questionDef.ID)
                              select new { cdfQD };
                return dbquery.AsEnumerable().Select(p => p.cdfQD).Distinct().ToList();
            }
            else
            {
                var dbquery = from cdfList in Context.CDFList.Where(c => c.ObjectTypeREFID == objectTypeREFID && !c.IsDeleted && c.TaskDefinitionIDs == null && c.Type == CDFListType.FormFields)
                              from questionDef in Context.QuestionDefinition.Where(x => x.TypeCode == questionType && !x.IsDeleted &&
                              (x.DecimalPlaces == 0 || x.DecimalPlaces == null))
                              from cdfQD in Context.CDFListQuestionDefinition.Where(a => a.CDFListID == cdfList.ID && a.QuestionDefinitionID == questionDef.ID)
                              select new { cdfQD };
                return dbquery.AsEnumerable().Select(p => p.cdfQD).Distinct().ToList();
            }

        }

        public TaskMappedField GetReferenceDataValuesForMappedField(long taskMappedFieldID)
        {
            return Context.TaskMappedFields.SingleOrDefault(t => t.ID == taskMappedFieldID);
        }

	    public QuestionDefinition GetQuestionDefinitionByID(long? questionId)
	    {
		    return Context.QuestionDefinition.SingleOrDefault(q => q.ID == questionId);
	    }

        public ReferenceDataValue GetReferenceDataForFieldCategory(string code)
        {
            ReferenceDataValue refData = Context.ReferenceDataValue.SingleOrDefault(r => r.Code == code);
            if (refData == null)
            {
                refData = Context.ReferenceDataValue.SingleOrDefault(x => x.DisplayValue == code);
            }
            return refData;
        }

	    public ReferenceDataValue GetReferenceDataForFieldCategoryByQuestionDefinitionID(long questionDefinitionID)
	    {
		    CDFListQuestionDefinition cdfLQD = Context.CDFListQuestionDefinition.SingleOrDefault(x => x.QuestionDefinitionID == questionDefinitionID);
		    ReferenceDataValue refData = Context.ReferenceDataValue.SingleOrDefault(x => x.ID == cdfLQD.FieldCategoryREFID);
		    return refData;
	    }

        public List<QuestionDefinition> GetQuestionDefinitionsByParentID(long? id)
        {
            return Context.QuestionDefinition.Where(x => x.ParentQuestionId == id && !x.IsDeleted).ToList();
        }

        public CDFListQuestionDefinition GetListQuestionDefinitionByQuestionDefinitionID(long? id)
        {
            return Context.CDFListQuestionDefinition.Include(c=> c.CDFList).SingleOrDefault(x => x.QuestionDefinitionID == id);
        }
        /// <summary>
        /// This method is called in order to check whether CDF values entered for a specified IRISObject, matching to the specific subclass 1/2/3 level
        /// between the IRISObject and the CDFList.
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <param name="matchSubClass1"></param>
        /// <param name="matchSubClass2"></param>
        /// <param name="matchSubClass3"></param>
        /// <returns></returns>
        public bool HasCDFAnswers(long irisObjectId, bool matchSubClass1 = false, bool matchSubClass2 = false, bool matchSubClass3 = false)
        {

            var dbquery = from answer in Context.Answer
                          join irisObject in Context.IRISObject on answer.IRISObjectID equals irisObject.ID
                          join questionDefinition in Context.QuestionDefinition on answer.QuestionID equals questionDefinition.ID
                          join cdfLQD in Context.CDFListQuestionDefinition on questionDefinition.ID equals cdfLQD.QuestionDefinitionID
                          join cdflist in Context.CDFList on cdfLQD.CDFListID equals cdflist.ID
                          where answer.IRISObjectID == irisObjectId &&
                          !answer.IsDeleted &&
                          !questionDefinition.IsDeleted &&
                          !cdflist.IsDeleted
                          select new
                          {
                              IRISObjectSubClass1 = irisObject.SubClass1ID,
                              IRISObjectSubClass2 = irisObject.SubClass2ID,
                              IRISObjectSubClass3 = irisObject.SubClass3ID,
                              CDFListSubClass1 = cdflist.SubClassification1REFID,
                              CDFListSubClass2 = cdflist.SubClassification2REFID,
                              CDFListSubClass3 = cdflist.SubClassification3REFID
                          };

            if (matchSubClass1)
                dbquery = dbquery.Where(x => x.IRISObjectSubClass1 != null && x.IRISObjectSubClass1 == x.CDFListSubClass1);

            if (matchSubClass2)
                dbquery = dbquery.Where(x => x.IRISObjectSubClass2 != null && x.IRISObjectSubClass2 == x.CDFListSubClass2);

            if (matchSubClass3)
                dbquery = dbquery.Where(x => x.IRISObjectSubClass3 != null && x.IRISObjectSubClass3 == x.CDFListSubClass3);


            return dbquery.Count() > 0;
        }




        /// <summary>
        ///    This method is called in order to check whether a particular CDF answer already exist
        /// </summary>
        public bool DoesCDFAnswerExist(Answer answer)
        {
            return Context.Answer.Any(a => a.IRISObjectID == answer.IRISObjectID &&
                                           a.Row == answer.Row &&
                                           a.QuestionID == answer.QuestionID &&
                                           !a.IsDeleted);
        }

        /// <summary>
        ///    This method is called in order to check whether a particular CDF list matching the criteria already exist 
        /// </summary>
        public bool DoesCDFListExist(long cdfListId, string cdfType, long objectTypeREFID, long? subClassification1REFID,
                                     long? subClassification2REFID, long? subClassification3REFID)
        {
            return Context.CDFList.Any(cdf => cdf.ID != cdfListId &&
                                           cdf.Type == cdfType &&
                                           cdf.ObjectTypeREFID == objectTypeREFID &&
                                           (cdf.SubClassification1REFID == null ?
                                                subClassification1REFID == null :
                                                cdf.SubClassification1REFID == subClassification1REFID) &&
                                           (cdf.SubClassification2REFID == null ?
                                                subClassification2REFID == null :
                                                cdf.SubClassification2REFID == subClassification2REFID) &&
                                           (cdf.SubClassification3REFID == null ?
                                                subClassification3REFID == null :
                                                cdf.SubClassification3REFID == subClassification3REFID) &&
                                           !cdf.IsDeleted);

        }

        // Survey Administration

        public Survey GetSurveyById(long id)
        {
            return Context.Surveys.Single(x => x.ID == id);
        }

        public Survey GetSurveyByName(string name)
        {
            return Context.Surveys.SingleOrDefault(x => x.Name == name);
        }

        public List<Survey> GetSurveys()
        {
            return Context.Surveys.Include(x => x.Owner).ToList();
        }

        public SurveyCategory GetSurveyCategoryById(long id)
        {
            return Context.SurveyCategories.Single(x => x.ID == id);
        }

        public SurveyCategory GetSurveyCategoryByName(string name)
        {
            return Context.SurveyCategories.SingleOrDefault(x => x.Name == name);
        }

        public List<SurveyCategory> GetSurveyCategoriesBySurveyId(long id)
        {
            return Context.SurveyCategories.Where(x => x.SurveyID == id).ToList();
        }

        public SurveyCategory GetSurveyCategoryByIdWithQuestions(long id)
        {
            ////Note: AsEnumerable is important without it the left joins will be ignored so surveryCategoryQuestionDefinition and questionDefinition will be null.
            ////http://eprystupa.wordpress.com/2009/11/26/linq-to-sql-tricks-building-efficient-queries-that-include-reference-data-or-child-entities/
            var query = (from surveyCategory in Context.SurveyCategories.Where(x => x.ID == id)
                         let surveyCategoryQuestionDefinitions = surveyCategory.SurveyCategoryQuestionDefinitions.Where(x => !x.QuestionDefinition.IsDeleted)
                         let questionDefinitions = surveyCategoryQuestionDefinitions.Select(x => x.QuestionDefinition)
                         select new { surveyCategory, surveyCategoryQuestionDefinitions, questionDefinitions }).AsEnumerable();

            var result = query.Select(x => x.surveyCategory).SingleOrDefault();
            result.TrackAll();
            return result;
        }

        // Observation Survey's

        public List<Survey> GetAvailableSurveys()
        {
            return Context.Surveys.Where(x => x.StartDate <= DateTime.Today && (!x.EndDate.HasValue || x.EndDate >= DateTime.Today)).ToList();
        }

        public SurveyCategory GetSurveyCategoryByIdWithQuestionsAndAnswers(long surveyCategoryId, long irisObjectId)
        {
            var query = (from surveyCategory in Context.SurveyCategories.Where(x => x.ID == surveyCategoryId && x.Active)
                         let surveyCategoryQuestionDefinitions = surveyCategory.SurveyCategoryQuestionDefinitions.Where(x => !x.QuestionDefinition.IsDeleted)
                         let questionDefinitions = surveyCategoryQuestionDefinitions.Select(x => x.QuestionDefinition)
                         let answers = questionDefinitions.Select(qd => qd.Answers.Where(a => !a.IsDeleted && a.IRISObjectID == irisObjectId))
                         select new { surveyCategory, surveyCategoryQuestionDefinitions, questionDefinitions, answers }).AsEnumerable();

            var result = query.Select(x => x.surveyCategory).SingleOrDefault();
            result.TrackAll();
            return result;
        }

        public List<SurveyCategory> GetSurveyCategoriesWithQuestionsAndAnswersBySurveyID(long surveyId, long irisObjectId)
        {
            var query = (from surveyCategory in Context.SurveyCategories.Where(x => x.SurveyID == surveyId && x.Active)
                         let surveyCategoryQuestionDefinitions = surveyCategory.SurveyCategoryQuestionDefinitions.Where(x => !x.QuestionDefinition.IsDeleted)
                         let questionDefinitions = surveyCategoryQuestionDefinitions.Select(x => x.QuestionDefinition)
                         let answers = questionDefinitions.Select(qd => qd.Answers.Where(a => !a.IsDeleted && a.IRISObjectID == irisObjectId))
                         select new { surveyCategory, surveyCategoryQuestionDefinitions, questionDefinitions, answers }).AsEnumerable();

            var result = query.Select(x => x.surveyCategory).OrderBy(x => x.Name).ToList();
            result.TrackAll();
            return result;

        }

        public List<long> DeleteAnswersBySurveyIdIrisObjectId(long surveyId, long irisObjectId)
        {
            var query = (from surveyCategory in Context.SurveyCategories.Where(x => x.SurveyID == surveyId)
                         let surveyCategoryQuestionDefinitions = surveyCategory.SurveyCategoryQuestionDefinitions
                         let questionDefinitions = surveyCategoryQuestionDefinitions.Select(x => x.QuestionDefinition)
                         let answer = questionDefinitions.Select(x => x.Answers.Where(y => y.IRISObjectID == irisObjectId))
                         select answer).SelectMany(x => x).SelectMany(x => x).Select(x => x.ID);
            var result = query.ToList();

            var items = Context.Answer.Where(x => result.Contains(x.ID)).ToList().TrackAll();
            items.ToList().ForEach(x =>
            {
                x.IsDeleted = true;
                ApplyEntityChanges(x);
            });
            SaveChanges();
            return result;
        }

        public List<long> DeleteAnswersByListIDIrisObjectId(long ListID, long irisObjectId)
        {
            var query = (from cdfList in Context.CDFList.Where(x => x.ID == ListID && x.IsDeleted==false)
                let CDFListQuestionDefinition = cdfList.CDFListQuestionDefinitions
                let questionDefinitions = CDFListQuestionDefinition.Select(x => x.QuestionDefinition)
                let answer = questionDefinitions.Where(x=>x.IsDeleted==false).
                                                                      Select(x => x.Answers.Where(y => y.IRISObjectID == irisObjectId && y.IsDeleted==false))
                select answer).SelectMany(x => x).SelectMany(x => x).Select(x => x.ID);
            var result = query.ToList();

            var items = Context.Answer.Where(x => result.Contains(x.ID)).ToList().TrackAll();
            items.ToList().ForEach(x =>
            {
                x.IsDeleted = true;
                ApplyEntityChanges(x);
            });
            SaveChanges();
            return result;
        }

        public List<CDFList> GetCDFListByObjectTypeAndSubClass(long objectTypeID, long? subClass1ID)
        {
            var cdfLists = Context.CDFList.Include("CDFListQuestionDefinitions.QuestionDefinition.Answers").Where(x => x.ObjectTypeREFID == objectTypeID && x.SubClassification1REFID == subClass1ID && !x.IsDeleted).ToList();
            return cdfLists;
        }

        public QuestionDefinition GetQuestionDefinitionBySymbolName(string symbolName)
        {
            var questionDefinition = Context.QuestionDefinition.SingleOrDefault(x => x.SymbolName == symbolName && !x.IsDeleted);
            return questionDefinition;
        }

        public Answer GetAnswerByObjectAndQuestion(long objectId, long questionId)
        {
            var answer = Context.Answer.Include(i => i.QuestionDefinition).SingleOrDefault(x => x.IRISObjectID == objectId && x.QuestionID == questionId && !x.IsDeleted);
            return answer.TrackAll();
        }

        public int UpdateChildrenQuestionDefinitions(long questionDefinitionId, int orderNo)
        {
            CDFListQuestionDefinition lqd = GetListQuestionDefinitionByQuestionDefinitionID(questionDefinitionId);
            if (!CDFIsDeleted(lqd.CDFListID))
            {
                CDFList list = GetCDFList(lqd.CDFListID);
                QuestionDefinition qd = list.QuestionDefinitions.SingleOrDefault(x => x.ID == questionDefinitionId && !x.IsDeleted);
                qd.OrderNumber = list.QuestionDefinitions.Where(x => x.TaskMappedFieldID != null) .OrderByDescending(o => o.OrderNumber).First().OrderNumber + orderNo + 1;
                ApplyEntityChanges(qd);
                SaveChanges();
                return qd.OrderNumber;
            }
            else
            {
                return 0;
            }
        }

        public string GetSymbolNameForCDFAnswer(long questionId)
        {
            var qd = Context.QuestionDefinition.SingleOrDefault(x => x.ID == questionId && !x.IsDeleted);
            var symbolName = "";
            if (qd != null)
            {
                symbolName = qd.SymbolName;
            }
            return symbolName;
        }

        public bool CDFIsDeleted(long cdfListId)
        {
            CDFList list = Context.CDFList.Single(x => x.ID == cdfListId);
            if (list.IsDeleted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
