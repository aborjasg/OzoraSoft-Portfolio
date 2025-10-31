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
    public class OzoraSoft_Shared_DBContext : DbContext
    {
        public OzoraSoft_Shared_DBContext(DbContextOptions<OzoraSoft_Shared_DBContext> options) : base(options) { }

        // Shared DB:
        public DbSet<EventLog> EventLogs { get; set; }        

        public OzoraSoft_Shared_DBContext()
        {
        }

        // Initializer
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
    }
}
