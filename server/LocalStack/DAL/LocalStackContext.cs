using LocalStack.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalStack.DAL {
    public class LocalStackContext : DbContext {

        public LocalStackContext(DbContextOptions<LocalStackContext> _options) : base(_options) {
        }

        public DbSet<Item> Items { get; set; }
    }
}
