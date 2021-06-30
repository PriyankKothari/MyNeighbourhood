using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNeighbourhood.Persistent.DbObjects.General;
using MyNeighbourhood.Persistent.DbObjects.Geographic;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbContexts
{
    public interface INeighbourhoodContext
    {
        DbSet<Country> Countries { get; set; }

        DbSet<State> States { get; set; }

        DbSet<District> Districts { get; set; }

        DbSet<City> Cities { get; set; }

        DbSet<Suburb> Suburbs { get; set; }

        DbSet<Neighbourhood> Neighbourhoods { get; set; }

        DbSet<Member> Members { get; set; }

        DbSet<Address> Addresses { get; set; }

        DbSet<Contact> Contacts { get; set; }

        DbSet<Email> Emails { get; set; }

        DbSet<Name> Names { get; set; }

        DbSet<Phone> Phones { get; set; }

        DbSet<Website> Websites { get; set; }

        int SaveChanges(bool acceptAllChangesOnSuccess = true);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}