using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources.Transit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources
{
    public class OzoraSoft_Transit_DBContext : DbContext
    {
        public OzoraSoft_Transit_DBContext(DbContextOptions<OzoraSoft_Transit_DBContext> options) : base(options) { }

        // DB Objects:
        public DbSet<VideoDevice> VideoDevices { get; set; }
        public DbSet<VideoCapture> VideoCaptures { get; set; }

        public OzoraSoft_Transit_DBContext()
        {
        }

        // Initializer
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }
}
