using System;
using System.Data.Common;
using System.Data.Objects;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess
{
    public partial interface IObjectContext : IDisposable
    {
        ObjectSet<T> EntitySet<T>() where T : class, IDomainObjectBase;

        void AcceptAllChanges();

        void ApplyPropertyChanges(string entitySetName, object changed);

        int SaveChanges();

        int SaveChanges(bool acceptChangesDuringSave);

        int SaveChanges(SaveOptions options);

        void DetectChanges();

        void CreateDatabase();

        void DeleteDatabase();

        bool DatabaseExists();

        string CreateDatabaseScript();

        DbConnection Connection { get; }

        int ExecuteFunction(string functionName, params ObjectParameter[] parameters);

    }
}
