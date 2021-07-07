using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class SmiteNewsModel
    {
        public string version { get; set; }
        //public VisibleContent homePageRotation { get; set; }
        //public VisibleContent storeFeatured { get; set; }
        //public VisibleContent storeTopSeller { get; set; }
        //public VisibleContent ultimateGodPackArt { get; set; }
        //public JustContent loginBlocker { get; set; }
        //public VisibleContent homePageModel { get; set; }
        //public JustContent storeSales { get; set; }
        public VisibleDifContent singlePanel { get; set; }
        //public VisibleContent ptsPanel { get; set; }
        public JustContent events { get; set; }
        //public VisibleContent patchOverview { get; set; }
        //public VisibleContent esportsLiveOverride { get; set; }
        //public PublicStatusMessages publicStatusMessages { get; set; }
    }
    public class Header
    {
        public string @default { get; set; }
        public string DEU { get; set; }
        public string FRA { get; set; }
        public string RUS { get; set; }
        public string POL { get; set; }
        public string TUR { get; set; }
        public string POR { get; set; }
        public string ESL { get; set; }
        public string CHN { get; set; }
        public string JPN { get; set; }
        public object INT { get; set; }
        public object ESN { get; set; }
    }

    public class ImageUrl
    {
        public string INT { get; set; }
    }
    public class Data
    {
        public string @default { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public int sysCounterId { get; set; }
        public int probability { get; set; }
        public Header header { get; set; }
        public int type { get; set; }
        public Header data { get; set; }
        public string imageUrl { get; set; }
        public int locationId { get; set; }
        public int? minLevel { get; set; }
        public int? maxLevel { get; set; }
        public List<int> hideIfItemOwned { get; set; }
        public Header data2 { get; set; }
        public List<object> showIfItemOwned { get; set; }
        public string imgOnly { get; set; }
        public List<int> hideIfItemOwnedOr { get; set; }
        public List<int> hideIfItemOwnedAnd { get; set; }
        public Header name { get; set; }
        public Header desc { get; set; }
        public Header button { get; set; }
        public int showPlayerId { get; set; }
        public int skinId { get; set; }
        public int cameraPos { get; set; }
        public string productId { get; set; }
        public int percentOff { get; set; }
        public string regions { get; set; }
        public List<EventList> eventList { get; set; }
        public Header header0 { get; set; }
        public Header text0 { get; set; }
        public Header header1 { get; set; }
        public Header text1 { get; set; }
        public Header header2 { get; set; }
        public Header text2 { get; set; }
    }

    public class DifContent
    {
        public string id { get; set; }
        public int sysCounterId { get; set; }
        public int probability { get; set; }
        public string isStandard { get; set; }
        public string isSteam { get; set; }
        public string isDiscord { get; set; }
        public string isFacebook { get; set; }
        public string isGoogle { get; set; }
        public string isPSN { get; set; }
        public string isXbox { get; set; }
        public string isNintendo { get; set; }
        public int type { get; set; }
        public Data data { get; set; }
        public Data data2 { get; set; }
        public int locationId { get; set; }
        public ImageUrl imageUrl { get; set; }
        public Header header { get; set; }
        public int minLevel { get; set; }
        public int maxLevel { get; set; }
        public string hideIfOwned { get; set; }
        public string showIfOwned { get; set; }
        public List<int> hideIfItemOwned { get; set; }
    }

    public class VisibleContent
    {
        public string visible { get; set; }
        public List<Content> content { get; set; }
    }
    public class VisibleDifContent
    {
        public string visible { get; set; }
        public List<DifContent> content { get; set; }
    }

    public class JustContent
    {
        public List<Content> content { get; set; }
    }

    public class EventList
    {
        public Header header { get; set; }
        public Header desc { get; set; }
    }

    public class PublicStatusMessages
    {
        public List<object> messageList { get; set; }
    }
}
