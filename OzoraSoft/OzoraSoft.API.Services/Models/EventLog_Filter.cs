namespace OzoraSoft.API.Services.Models
{
    public class EventLog_Filter
    {
        public int project_id { get; set; } = 0;
        public int module_id { get; set; } = 0;
        public int controller_id { get; set; } = 0;
        public int action_id { get; set; } = 0;
        public string process_datetime { get; set; } = "";        
        public string user_name { get; set; } = "";
    }
}
