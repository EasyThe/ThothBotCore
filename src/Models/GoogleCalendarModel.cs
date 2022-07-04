using System;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class GoogleCalendarModel
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public DateTime updated { get; set; }
        public string timeZone { get; set; }
        public string accessRole { get; set; }
        public List<object> defaultReminders { get; set; }
        public string nextPageToken { get; set; }
        public List<Item> items { get; set; }
    }
    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string htmlLink { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public string iCalUID { get; set; }
        public int sequence { get; set; }
        public string eventType { get; set; }
        public string transparency { get; set; }
    }
    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }
}
