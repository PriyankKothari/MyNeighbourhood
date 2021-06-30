using System.Collections.Generic;
using System.Data.Objects;
using Datacom.IRIS.Common.Implementations;
using Datacom.IRIS.DomainModel.Domain;
using System;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IRepositoryBase : IDisposable
    {
        void EnsureDatabaseExists();
        
        void EnsureCleanDatabaseExists();
        
        void ApplyEntityChanges<T>(T entity) where T : class, IDomainObjectBase;
        
        void DetachEntity<T>(T entity) where T : class, IDomainObjectBase;
        
        void 
            AddEntity<T>(T entity) where T : class, IDomainObjectBase;
        
        void DeleteEntity<T>(T entity) where T : class, IDomainObjectBase;
        
        int SaveChanges(SaveOptions saveOptions);
        
        int SaveChanges();

        List<SQLResponse> SqlResponseList();
    }
}
