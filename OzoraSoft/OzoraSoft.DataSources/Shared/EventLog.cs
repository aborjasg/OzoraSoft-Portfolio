using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.DataSources.Shared
{
    public class EventLog
    {
        public int id { get; set; } = 0;
        public int project_id {  get; set; } = 0;
        public int module_id {  get; set; } = 0;
        public int controller_id {  get; set; } = 0;
        public int action_id {  get; set; } = 0;
        public string action_result {  get; set; } = "";
        public double process_duration { get; set; } = 0;
        public DateTime process_datetime { get; set; } = DateTime.Now;
        public bool visible { get; set; } = true;
        public string user_name { get; set; } = "";
        public string user_ip_address { get; set; } = "";
    }
}
