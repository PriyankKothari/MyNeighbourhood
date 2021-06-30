using Microsoft.EntityFrameworkCore;
using MyNeighbourhood.Persistent.DbObjects.General;
using MyNeighbourhood.Persistent.DbObjects.Geographic;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbContexts
{
    public class NeighbourhoodContext : DbContext, INeighbourhoodContext
    {
        public NeighbourhoodContext(DbContextOptions<NeighbourhoodContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<District> Districts { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Suburb> Suburbs { get; set; }

        public DbSet<Neighbourhood> Neighbourhoods { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Email> Emails { get; set; }

        public DbSet<Name> Names { get; set; }

        public DbSet<Phone> Phones { get; set; }

        public DbSet<Website> Websites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Country
            builder.Entity<Country>().ToTable("Country").HasKey(country => country.Id);
            builder.Entity<Country>().Property(country => country.Id).ValueGeneratedOnAdd();
            builder.Entity<Country>().HasQueryFilter(country => !country.IsDeleted);

            builder.Entity<Country>().HasMany(c => c.States).WithOne(s => s.Country).HasForeignKey(s => s.CountryId);

            //// Address
            //builder.Entity<Address>().ToTable("Address").HasKey(ad => ad.AddressId);
            //builder.Entity<Address>().Property(ad => ad.AddressId).ValueGeneratedOnAdd();
            //builder.Entity<Address>().Property(ad => ad.AddressType).HasConversion(type => type.ToStringValue(),
            //    type => type.ToEnumeration<AddressTypes>(true, true));

            //// Order
            //builder.Entity<Order>().ToTable("Order").HasKey(or => or.OrderId);
            //builder.Entity<Order>().Property(or => or.OrderId).ValueGeneratedOnAdd();
            //builder.Entity<Order>().Property(or => or.OrderStatus).HasConversion(status => status.ToStringValue(),
            //    status => status.ToEnumeration<OrderStatuses>(true, true));

            //// OrderItem
            //builder.Entity<OrderItem>().ToTable("OrderItem").HasKey(item => item.OrderItemId);
            //builder.Entity<Order>().Property(or => or.OrderId).ValueGeneratedOnAdd();


            //// Product
            //builder.Entity<Product>().ToTable("Product").HasKey(pr => pr.ProductId);
            //builder.Entity<Product>().Property(pr => pr.ProductId).ValueGeneratedOnAdd();

            //// Customer to Addresses
            //builder.Entity<Customer>()
            //    .HasMany(cs => cs.Addresses)
            //    .WithOne(ad => ad.Customer)
            //    .HasForeignKey(ad => ad.CustomerId);

            //// Customer to Orders
            //builder.Entity<Customer>()
            //    .HasMany(cs => cs.Orders)
            //    .WithOne(or => or.Customer)
            //    .HasForeignKey(or => or.CustomerId);

            //// Order to OrderItems
            //builder.Entity<Order>()
            //    .HasMany(or => or.OrderItems)
            //    .WithOne(item => item.Order)
            //    .HasForeignKey(item => item.OrderId);

            //// OrderItem to Product
            //builder.Entity<Product>()
            //    .HasOne(pr => pr.OrderItem)
            //    .WithOne(item => item.Product)
            //    .HasForeignKey<OrderItem>(item => item.ProductId);
        }
    }
}