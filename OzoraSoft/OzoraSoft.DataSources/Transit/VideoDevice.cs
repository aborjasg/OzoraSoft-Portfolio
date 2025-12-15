using System;
using System.Collections.Generic;
using System.Text;

namespace OzoraSoft.DataSources.Transit
{
    public class VideoDevice
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = "";
        public bool status { get; set; } = false;
        public DateTime created_date { get; set; } = DateTime.Now;
    }
}
