using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources.Transit
{
    public class VideoCapture
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = "";
        public bool status { get; set; } = false;
        public DateTime created_date { get; set; } = DateTime.Now;
    }
}
