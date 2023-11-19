using GrpcService.Classes;
using Microsoft.EntityFrameworkCore;

namespace GrpcService
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Column> Columns { get; set; } = null!;
        public DbSet<Database> Databases { get; set; } = null!;
        public DbSet<Row> Rows { get; set; } = null!;
        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<Classes.Type> Types { get; set; } = null!;

        public ApplicationContext() { }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=grpcdbdb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*modelBuilder.Entity<Database>().HasData(
                    new Database { Id = 1, DBName = "DB1" }
            );
            modelBuilder.Entity<Table>().HasData(
                    new Table { Id = 1, TableName = "Table1", DatabaseId = 1 }
            );
            modelBuilder.Entity<Classes.Type>().HasData(
                    new Classes.Type { Id = 1, Name = "TypeChar" },
                    new Classes.Type { Id = 2, Name = "TypeEmail" },
                    new Classes.Type { Id = 3, Name = "TypeEnum" },
                    new Classes.Type { Id = 4, Name = "TypeInteger" },
                    new Classes.Type { Id = 5, Name = "TypeReal" },
                    new Classes.Type { Id = 6, Name = "TypeString" }
            );
            modelBuilder.Entity<Column>().HasData(
                    new Column { Id = 1, TableId = 1, ColName = "Char", TypeId = 1 },
                    new Column { Id = 2, TableId = 1, ColName = "Email", TypeId = 2 },
                    new Column { Id = 3, TableId = 1, ColName = "Enum", TypeId = 3 }
            );
            modelBuilder.Entity<Row>().HasData(
                   new Row { Id = 1, TableId = 1, RowValues = "{\"Char\":\"g\",\"Email\":\"email1@gmail.com\",\"Enum\":\"1,5,8\"}" },
                   new Row { Id = 2, TableId = 1, RowValues = "{\"Char\":\"h\",\"Email\":\"email2@gmail.com\",\"Enum\":\"1,6\"}" },
                   new Row { Id = 3, TableId = 1, RowValues = "{\"Char\":\"y\",\"Email\":\"email3@gmail.com\",\"Enum\":\"yt,hg,j\"}" }
           );*/
        }
    }
}
