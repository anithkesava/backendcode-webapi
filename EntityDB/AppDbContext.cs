using Microsoft.EntityFrameworkCore;
using MyFirstWebAPI.Models;

namespace MyFirstWebAPI.EntityDB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Users> UsersData { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
