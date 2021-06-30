using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess
{
    public partial class IRISContext : IObjectContext
    {
        private bool _automaticallySetAuditFields = true;
        public bool AutomaticallySetAuditFields
        {
            private get
            {
                return _automaticallySetAuditFields;
            }
            set
            {
                _automaticallySetAuditFields = value;
            }
        }

        public string CurrentUserName;

        #region Repository Utility Methods

        private readonly Dictionary<Type, object> _entitySets = new Dictionary<Type, object>();

        /// <summary>
        ///    Returns an ObjectSet containing all IRIS Object sets that make the IRIS context. It
        ///    allows us to avoid having specific method signatures for each IRIS Object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ObjectSet<T> EntitySet<T>() where T : class, IDomainObjectBase {
        	var t = typeof(T);
        	object match;
        	
            if (!_entitySets.TryGetValue(t, out match)) {
        		match = CreateObjectSet<T>();
        		_entitySets.Add(t, match);
        	} 
        	
            return (ObjectSet<T>)match; 
        }

        #endregion

        #region Change Tracking

        /// <summary>
        ///    When the UI sends a modified graph to the service and then intends to continue working with the 
        ///    same graph (on the client), we have to manually iterate through the graph on the client and call 
        ///    the AcceptChanges method on each object to reset the change tracker.
        /// 
        ///    If that is not done, attaching an entity back to the context with a State of 'Added' will re-create
        ///    the objects in the DB and cause duplicates
        /// </summary>
        public void AcceptAllEntitySelfTrackingChanges()
        {
            foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged))
            {
                if (entry.Entity is IObjectWithChangeTracker)
                {
                    IObjectWithChangeTracker entity = (IObjectWithChangeTracker) entry.Entity;
                    entity.AcceptChanges();
                }
            }
        }

        #endregion 

        #region Auditing

        public override int SaveChanges(SaveOptions options)
        {
            var allowedToUpdateReferenceDataValueProperties =
                new[] {"code", "displayvalue", "displayorder", "description", "iscurrent" };

            var isProtected = string.Empty;
            var referenceDataValueId = string.Empty;

            // TODO: Remove the logging when the issue is fixed.
            // IRIS-828 - If the modified property is not allowed, log the original value and modified value with the call stack.
            foreach (var entry in ObjectStateManager.GetObjectStateEntries(EntityState.Modified))
            {
                if (entry.Entity is ReferenceDataValue)
                {   
                    var currentValues = entry.CurrentValues;
                    var originalValues = entry.OriginalValues;

                    isProtected = entry.CurrentValues.GetValue(entry.CurrentValues.GetOrdinal("IsProtected")).ToString();

                    referenceDataValueId = entry.CurrentValues.GetValue(entry.CurrentValues.GetOrdinal("ID")).ToString();

                    for (var i = 0; i < currentValues.FieldCount; i++)
                    {
                        var propertyName = currentValues.GetName(i);

                        // if property name matches the allowed properties, then continue
                        if (allowedToUpdateReferenceDataValueProperties.Contains(propertyName.ToLower()))
                            continue;

                        // log the trace if original value doesn't match with the current value
                        var originalValue = originalValues.GetValue(i).ToString();
                        var currentValue = currentValues.GetValue(i).ToString();

                        

                        if (!originalValue.Equals(currentValue))
                        {
                            var trace = new System.Diagnostics.StackTrace(true);
                            UtilLogger.Instance.LogError(
                                $"Property {propertyName} of ReferenceDataValue table is updated. Original value: '{originalValue}', Modified value: '{currentValue}', User: {SecurityHelper.CurrentUserName}, " +
                                $" ReferenceDataValueID : {referenceDataValueId}, IsProtected : {isProtected}  Call Stack is : {trace}");

                            //Reset ParentValueId,CollectionId,IsProtected back to original value as these should not change
                            currentValues.SetValue(i, Int64.Parse(originalValue));
                        }
                    }
                }
            }

            if (options == SaveOptions.None)
                return base.SaveChanges();
            
            foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Modified))
            {
                if (!entry.IsRelationship && entry.Entity is IDomainObjectBase)
                {
                    SetAuditingProperties(entry.Entity, entry.State);
                }
            }

            var objectCount = base.SaveChanges(options);

            if (objectCount > 0)
            {
                AcceptAllEntitySelfTrackingChanges();
            }

            return objectCount;
            
        }

        private void SetAuditingProperties(object domainObject, EntityState state)
        {
            /*
             * We will always want to set audit fields for every entity that will 
             * be persisted in our database.
             * 
             * In case of User Tasks, the CreatedBy user set by SecurityHelper.CurrentUserName
             * is the Workflow user. We don't want to display the name of the Workflow user on the front-end.
             * Hence, the front-end sets the value of CreatedBy to the currently logged in user.
             * When this happens, WorkflowDataRepository will set AutomaticallySetAuditFields to false
             * to avoid the CreatedBy value being overwritten here.
             * 
             * In case of System generated Note, the SecurityHelper.CurrentUserName is null in ansychronis 
             * document generation/upload processes. Thus, we set the audit fields in the DocumentService
             * where get passed user information during the ansychronis processes.
             * 
             */
            if (AutomaticallySetAuditFields)
            {
                string userName = SecurityHelper.CurrentUserName;

                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception("User identity could not be identified and audit fields could not be set");
                }
                
                DateTime nowDateTime = DateTime.Now;

                if (domainObject is IDomainChildObjectBase)
                {
                    IDomainChildObjectBase domainChildObjectCast = (IDomainChildObjectBase) domainObject;
                    if (state.Equals(EntityState.Added))
                    {
                        domainChildObjectCast.ChildDateCreated = nowDateTime;
                        domainChildObjectCast.ChildCreatedBy = userName;
                    }

                    domainChildObjectCast.ChildLastModified = nowDateTime;
                    domainChildObjectCast.ChildModifiedBy = userName;
                }

                IDomainObjectBase domainObjectCast = (IDomainObjectBase) domainObject;
                if (state.Equals(EntityState.Added))
                {
                    domainObjectCast.DateCreated = nowDateTime;
                    domainObjectCast.CreatedBy = userName;
                }

                domainObjectCast.LastModified = nowDateTime;
                domainObjectCast.ModifiedBy = userName;
            }
        }

        #endregion
    }
}
