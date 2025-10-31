using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Runtime.Serialization;

namespace OzoraSoft.Library.PictureMaker.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Key] public int Id { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public string Metadata { get; set; } = string.Empty; // Serialized
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartProcess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime EndProcess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Duration => EndProcess > StartProcess ? EndProcess.Subtract(StartProcess): TimeSpan.Zero;

        public string Length(string content)
        {
            return $"{content.Length} bytes / {Math.Round((decimal)content.Length / 1024, 2)} KB";
        }

        public ModelBase()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
