using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json;

namespace OzoraSoft.Library.PictureMaker.Models;

/// <summary>
///  
/// </summary>
public partial class RunImage : ModelBase
{
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string DataSource { get; set; } = string.Empty; // Derived data (serialized and compressed)
    /// <summary>
    /// 
    /// </summary>
    public string Content { get; set; } = string.Empty; // PNG image (compressed)

    public RunImage() : base()
    {
        StartProcess = DateTime.Now;
    }

    public void LoadData(string name, RunMetadata metadata, DerivedData derivedData, string plotImage)
    {        
        Name = name;
        Metadata = JsonConvert.SerializeObject(metadata);
        DataSource = UtilsForMessages.Compress(JsonConvert.SerializeObject(derivedData));
        Content = UtilsForMessages.Compress(plotImage);
    }
}
