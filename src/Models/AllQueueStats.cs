
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class AllQueueStats
    {
        public string queueName { get; set; }
        public int matches { get; set; }
        public List<QueueStats> queueStats { get; set; }
    }
}
