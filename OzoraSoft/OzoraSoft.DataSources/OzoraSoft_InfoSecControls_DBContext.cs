using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources.InfoSecControls;
using OzoraSoft.DataSources.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources
{
    public class OzoraSoft_InfoSecControls_DBContext : DbContext
    {
        public OzoraSoft_InfoSecControls_DBContext(DbContextOptions<OzoraSoft_InfoSecControls_DBContext> options) : base(options) { }

        // InfoSecControls DB:
        public DbSet<SystemParameter> SystemParameters { get; set; }
        public DbSet<OrganizationPolicy> OrganizationPolicies { get; set; }

        public OzoraSoft_InfoSecControls_DBContext()
        {
        }

        // Initializer
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
    }
}
