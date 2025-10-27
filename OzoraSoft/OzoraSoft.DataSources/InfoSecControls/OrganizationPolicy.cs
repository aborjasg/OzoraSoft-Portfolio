using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources.InfoSecControls
{
    public class OrganizationPolicy
    {
        [Key]
        public int Id { get; set; } = 0;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Information_Security_Control_Id { get; set; } = 0;
        public string? Control_Type { get; set; }
        public string? Information_Security_Properties { get; set; }
        public string? Cybersecurity_Concepts { get; set; }
        public string? Operational_Capabilities { get; set; }
        public string? Security_Domains { get; set; } 
        public bool Active { get; set; } = false;
    }
}
