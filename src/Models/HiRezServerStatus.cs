namespace ThothBotCore.Models
{
    public class HiRezServerStatus
    {
        public string Entry_datetime { get; set; }
        public string Environment { get; set; }
        public bool Limited_access { get; set; }
        public string Platform { get; set; }
        public object Ret_msg { get; set; }
        public string Status { get; set; }
        public string Version { get; set; }
    }
}
