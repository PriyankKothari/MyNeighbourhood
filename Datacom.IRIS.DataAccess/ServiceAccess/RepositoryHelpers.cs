using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    /// <summary>Generic repository helper methods</summary>
    public static class RepositoryHelpers
    {
        /// <summary>Load the plans, rules and policies for an entity.</summary>
        /// <param name="context">DB Context to query</param>
        /// <param name="entity">entity that has Plans, Rules and Policies</param>
        public static void LoadPlansRulesPoliciesForEntity(IObjectContext context, IDomainObjectBase entity)
        {
            //NOTE: This was made a common function because the pattern was under going variations in the code base some of which did not work.
            //      Please change all patterns if you can improve the query, thx.
            //TODO  It have been would be better to build this query dynamically, but this was a pragmatic time/cost choice not be DRY.

            if (entity is Activity)
            {
                LoadPlansRulesPoliciesForActivity(context, entity);
            }
            else if (entity is Authorisation)
            {
                LoadPlansRulesPoliciesForAuthorisation(context, entity);
            }
            else if (entity is RegimeMngt)
            {
                LoadPlansRulesPoliciesForRegimeMngt(context, entity);
            }
            else if (entity is RegimeEnvironment)
            {
                LoadPlansRulesPoliciesForRegimeEnvironment(context, entity);
            }
            else if (entity is Request)
            {
                LoadPlansRulesPoliciesForRequest(context, entity);
            }
            else if (entity is MngtSiteLandManagement)
            {
                LoadPlansRulesPoliciesForMngtSiteLandManagement(context, entity);
            }
            else if (entity is EnforcementAllegedOffence)
            {
                LoadPlansRulesPoliciesForEnforcementAllegedOffence(context, entity);
            }
            else
            {
                throw new NotImplementedException(string.Format("Could not find an implementation to load Plans, Rules and Policies for {0}, please add one.", entity.GetType().Name));    
            }

            //TODO  CODE/NOTES FROM FAILED SPIKE.
            //NOTE: I only got as far as building joining two separate queries, but would have like to build it from fluent or dynamic linq expressions.
            //NOTE: I was initially hoping that Linq to Entities supported Dynamic where expression for joins that would easily allow me to write a dynamic query e.g. 
            //NOTE:     var dbquery = from entityPlan in context.RegimeMngtPlan.Where(p => p.RegimeMngtID == item.ID)
            //NOTE:                   //from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
            //NOTE:                   from plan in context.Plans.Where("plan.ID = entityPlan.PlanID")   <=== SEE STRING DYNAMICS this would be the desired solution, where Linq to Entity provider parse the correct expression tree fragment.
            //NOTE:                   from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)

            //var regimeMngt = entity as RegimeMngt;

            //var queryableEnityPlans =
            //    from entityPlan in Context.RegimeMngtPlan.Where(p => p.RegimeMngtID == regimeMngt.ID)
            //    from plan in Context.Plans.Where(p => p.ID == entityPlan.PlanID)
            //    select new { entityPlan, plan };                 

            //var queryableEnityPlansWithRulesAndPolicies = from anon in queryableEnityPlans

            //join planRef in Context.ReferenceDataValue on anon.plan.PlanREFID equals planRef.ID into planRefs
            //from planRef in planRefs

            //join rule in Context.RuleObjectives.Where(r => !r.IsDeleted) on anon.plan.ID equals rule.PlanID into rules
            //from rule in rules

            //from ruleRef in Context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
            //from policy in Context.Policies.Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted).DefaultIfEmpty()
            //from policyRef in Context.ReferenceDataValue.Where(rdv => rdv.ID == policy.PolicyREFID).DefaultIfEmpty()

            //select new { anon.entityPlan, anon.plan, rule, policy, planRef, ruleRef, policyRef };

            //regimeMngt.RegimeMngtPlans.AddRange(queryableEnityPlansWithRulesAndPolicies.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }
        private static void LoadPlansRulesPoliciesForEnforcementAllegedOffence(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as EnforcementAllegedOffence;
            if (item == null) return;
            var dbquery = from entityPlan in context.EnforcementAllegedOffencePlans.Where(p => p.EnforcementAllegedOffenceID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new { entityPlan, plan, rule, policy, refData1, refData2, refData3 };

            item.EnforcementAllegedOffencePlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }

        private static void LoadPlansRulesPoliciesForRequest(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as Request;
            if (item == null) return;
            var dbquery = from entityPlan in context.RequestPlan.Where(p => p.RequestID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new { entityPlan, plan, rule, policy, refData1, refData2, refData3 };
            
            item.RequestPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }


        private static void LoadPlansRulesPoliciesForAuthorisation(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as Authorisation;
            if (item == null) return;
            var dbquery = from entityPlan in context.AuthorisationPlans.Where(p => p.AuthorisationID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new { entityPlan, plan, rule, policy, refData1, refData2, refData3 };

            item.AuthorisationPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }

        private static void LoadPlansRulesPoliciesForActivity(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as Activity;
            if (item == null) return;
            var dbquery = from entityPlan in context.ActivityPlans.Where(p => p.ActivityID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new { entityPlan, plan, rule, policy, refData1, refData2, refData3 };

            item.ActivityPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }

        private static void LoadPlansRulesPoliciesForRegimeEnvironment(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as RegimeEnvironment;
            if (item == null) return;
            var dbquery = from entityPlan in context.RegimeEnvironmentPlan.Where(p => p.RegimeEnvironmentID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new { entityPlan, plan, rule, policy, refData1, refData2, refData3 };

            item.RegimeEnvironmentPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }

        private static void LoadPlansRulesPoliciesForRegimeMngt(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as RegimeMngt;
            if (item == null) return;
            var dbquery = from entityPlan in context.RegimeMngtPlans.Where(p => p.RegimeMngtID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new {entityPlan, plan, rule, policy, refData1, refData2, refData3};

            item.RegimeMngtPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }

        private static void LoadPlansRulesPoliciesForMngtSiteLandManagement(IObjectContext context, IDomainObjectBase entity)
        {
            var item = entity as MngtSiteLandManagement;
            if (item == null) return;
            var dbquery = from entityPlan in context.MngtSiteLandManagementPlans.Where(p => p.MngtSiteLandManagementID == item.ID)
                          from plan in context.Plans.Where(p => p.ID == entityPlan.PlanID)
                          from refData1 in context.ReferenceDataValue.Where(rdv => rdv.ID == plan.PlanREFID)
                          from rule in context.RuleObjectives.Where(r => r.PlanID == plan.ID && !r.IsDeleted)
                          from refData2 in context.ReferenceDataValue.Where(rdv => rdv.ID == rule.RuleREFID)
                          from policy in context.Policies
                              .Where(p => p.RuleObjectiveID == rule.ID && !p.IsDeleted)
                              .DefaultIfEmpty()
                          from refData3 in context.ReferenceDataValue
                              .Where(rdv => rdv.ID == policy.PolicyREFID)
                              .DefaultIfEmpty()
                          select new {entityPlan, plan, rule, policy, refData1, refData2, refData3};

            item.MngtSiteLandManagementPlans.AddRange(dbquery.AsEnumerable().Select(s => s.entityPlan).Distinct().ToList());
        }
        
        public static void AddTrackableCollection<T>(this TrackableCollection<T> toList, List<T> fromList) where T : IDomainObjectBase
        {
            fromList.ForEach(u => toList.Add(u));
        }
    }


}
