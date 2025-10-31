using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.PictureMaker.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ActionResponse
    {
        public int Id { get; set; } = 0;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Duration => EndDate.Subtract(StartDate);
        public string ContentLenght => $"{Content.Length} bytes / {Math.Round((decimal)Content.Length / 1024, 2)} KB";

        public ActionResponse()
        {
            StartDate = DateTime.Now;
            Type = "Information";
        }
    }
}
