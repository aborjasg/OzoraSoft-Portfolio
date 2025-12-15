using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources.Transit
{
    public class VideoCapture
    {
        public int id { get; set; } = 0;
        public int videodevice_id { get; set; } = 0;        
        public bool status { get; set; } = false;
        public DateTime created_date { get; set; } = DateTime.Now;

        public string ContentLenght =>
            image != null
            ? $"{image.Length} bytes / {Math.Round((decimal)image.Length / 1024, 2)} KB"
            : "No image";

        public byte[]? image { get; set; }
    }
}
