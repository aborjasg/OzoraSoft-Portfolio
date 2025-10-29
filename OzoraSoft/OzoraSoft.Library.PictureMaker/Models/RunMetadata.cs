using Newtonsoft.Json;

namespace OzoraSoft.Library.PictureMaker.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RunMetadata : MetadataBase
    {
        public string Name { get; set; } = string.Empty;

        public RunMetadata() : base() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        public RunMetadata(string name, PictureTemplate template) : this()
        {
            Name = name;
            Metadata = JsonConvert.SerializeObject(template);
        }
    }
}
