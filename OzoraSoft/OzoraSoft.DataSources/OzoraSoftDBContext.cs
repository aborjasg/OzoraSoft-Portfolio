using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources.InfoSecControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources
{
    public class OzoraSoftDBContext : DbContext
    {
        public OzoraSoftDBContext(DbContextOptions<OzoraSoftDBContext> options) : base(options) { }

        // Models
        public DbSet<SystemParameter> SystemParameters { get; set; }
        public DbSet<OrganizationPolicy> OrganizationPolicies { get; set; }

        public OzoraSoftDBContext()
        {
        }

        // Initializer
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
    }
}
