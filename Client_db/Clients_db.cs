using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Client_db
{
    public class Clients_db : DbContext
    {
        public Clients_db()
        {
            /*this.Database.EnsureDeleted();
            this.Database.EnsureCreated();*/
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"
                                            Data Source = (localdb)\MSSQLLocalDB;
                                            Initial Catalog = MyClientsDB;
                                            Integrated Security = True;"
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Client>()
                .Property(x => x.Login)
                .IsRequired(true)
                .HasMaxLength(50);

            modelBuilder.Entity<Client>()
                .Property(a => a.Password)
                .IsRequired(true)
                .HasMaxLength(50);
            modelBuilder.Entity<Client>()
                .Property(a => a.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            modelBuilder.Entity<Contact>()
                .HasOne(x => x.Client)
                .WithMany(m => m.Contacts)
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<Contact>()
                .HasIndex(x => new { x.ClientId, x.ContactClientId })
                .IsUnique();

        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }

    }

    public class Client
    {
        public Client()
        {
            Contacts = new HashSet<Contact>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public ICollection<Contact> Contacts { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ContactClientId { get; set; }
        public Client Client { get; set; }
    }
}
