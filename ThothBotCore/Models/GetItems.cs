using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class GetItems
    {
        public class Menuitem
        {
            public string Description { get; set; }
            public string Value { get; set; }
        }

        public class ItemDescription
        {
            public string Description { get; set; }
            public List<Menuitem> Menuitems { get; set; }
            public object SecondaryDescription { get; set; }
        }

        public class Item
        {
            public string ActiveFlag { get; set; }
            public int ChildItemId { get; set; }
            public string DeviceName { get; set; }
            public int IconId { get; set; }
            public ItemDescription ItemDescription { get; set; }
            public int ItemId { get; set; }
            public int ItemTier { get; set; }
            public int Price { get; set; }
            public string RestrictedRoles { get; set; }
            public int RootItemId { get; set; }
            public string ShortDesc { get; set; }
            public bool StartingItem { get; set; }
            public string Type { get; set; }
            public string itemIcon_URL { get; set; }
            public object ret_msg { get; set; }
        }
    }
}
