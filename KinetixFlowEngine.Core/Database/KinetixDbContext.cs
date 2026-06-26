using KinetixFlowEngine.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public class KinetixDbContext : DbContext
    {
        public KinetixDbContext(
        DbContextOptions<KinetixDbContext> options)
        : base(options)
        {
        }


        public DbSet<MarketStateEntity> MarketStates => Set<MarketStateEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(KinetixDbContext).Assembly);
        }
    }
}
