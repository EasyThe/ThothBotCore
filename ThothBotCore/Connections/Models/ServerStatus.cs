using System;
using System.Collections.Generic;

namespace ThothBotCore.Connections.Models
{
    public class ServerStatus
    {
        public Page page { get; set; }
        public List<Component> components { get; set; }
        public List<Incident> incidents { get; set; }
        public List<ScheduledMaintenances> scheduled_maintenances { get; set; }
        public Status status { get; set; }
    }

    public class Page
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string time_zone { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Status
    {
        public string indicator { get; set; }
        public string description { get; set; }
    }

    public class Incident
    {
        public string name { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object monitoring_at { get; set; }
        public object resolved_at { get; set; }
        public string shortlink { get; set; }
        public string id { get; set; }
        public string page_id { get; set; }
        public List<IncidentUpdate> incident_updates { get; set; }
        public List<Component2> components { get; set; }
        public string impact { get; set; }
    }

    public class Component
    {
        public string status { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int position { get; set; }
        public object description { get; set; }
        public bool showcase { get; set; }
        public string id { get; set; }
        public string group_id { get; set; }
        public string page_id { get; set; }
        public bool group { get; set; }
        public bool only_show_if_degraded { get; set; }
        public List<string> components { get; set; }
    }

    public class ScheduledMaintenances
    {
        public string name { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object monitoring_at { get; set; }
        public object resolved_at { get; set; }
        public string shortlink { get; set; }
        public DateTime scheduled_for { get; set; }
        public DateTime scheduled_until { get; set; }
        public string id { get; set; }
        public string page_id { get; set; }
        public List<IncidentUpdate> incident_updates { get; set; }
        public List<Component2> components { get; set; }
        public string impact { get; set; }
    }

    public class IncidentUpdate
    {
        public string status { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime display_at { get; set; }
        public List<AffectedComponent> affected_components { get; set; }
        public bool deliver_notifications { get; set; }
        public long tweet_id { get; set; }
        public string id { get; set; }
        public string incident_id { get; set; }
        public object custom_tweet { get; set; }
    }

    public class Component2
    {
        public string status { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int position { get; set; }
        public object description { get; set; }
        public bool showcase { get; set; }
        public string id { get; set; }
        public string page_id { get; set; }
        public string group_id { get; set; }
        public bool group { get; set; }
        public bool only_show_if_degraded { get; set; }
    }

    public class AffectedComponent
    {
        public string code { get; set; }
        public string name { get; set; }
        public string old_status { get; set; }
        public string new_status { get; set; }
    }
}
