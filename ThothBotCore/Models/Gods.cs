using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    [BsonIgnoreExtraElements]
    public class Gods
    {
        public class DescriptionValue
        {
            public string description { get; set; }
            public string value { get; set; }
        }

        public class ItemDescription
        {
            public string cooldown { get; set; }
            public string cost { get; set; }
            public string description { get; set; }
            public List<DescriptionValue> menuitems { get; set; }
            public List<DescriptionValue> rankitems { get; set; }
            public string secondaryDescription { get; set; }
        }

        public class Description
        {
            public ItemDescription itemDescription { get; set; }
        }

        public class Ability
        {
            public Description Description { get; set; }
            public int Id { get; set; }
            public string Summary { get; set; }
            public string URL { get; set; }
            public string Emoji { get; set; }
            public int DomColor { get; set; }
            public string Video { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class God
        {
            public Ability Ability_1 { get; set; }
            public Ability Ability_2 { get; set; }
            public Ability Ability_3 { get; set; }
            public Ability Ability_4 { get; set; }
            public Ability Ability_5 { get; set; }
            public double AttackSpeed { get; set; }
            public double AttackSpeedPerLevel { get; set; }
            public string Cons { get; set; }
            public double HP5PerLevel { get; set; }
            public double Health { get; set; }
            public double HealthPerFive { get; set; }
            public double HealthPerLevel { get; set; }
            public string Lore { get; set; }
            public double MP5PerLevel { get; set; }
            public double MagicProtection { get; set; }
            public double MagicProtectionPerLevel { get; set; }
            public double MagicalPower { get; set; }
            public double MagicalPowerPerLevel { get; set; }
            public double Mana { get; set; }
            public double ManaPerFive { get; set; }
            public double ManaPerLevel { get; set; }
            public string Name { get; set; }
            public string OnFreeRotation { get; set; }
            public string Pantheon { get; set; }
            public double PhysicalPower { get; set; }
            public double PhysicalPowerPerLevel { get; set; }
            public double PhysicalProtection { get; set; }
            public double PhysicalProtectionPerLevel { get; set; }
            public string Pros { get; set; }
            public string Roles { get; set; }
            public double Speed { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public Description basicAttack { get; set; }
            public string godCard_URL { get; set; }
            public string godHeader_URL { get; set; }
            public string godIcon_URL { get; set; }
            public int id { get; set; }
            public string latestGod { get; set; }
            public object ret_msg { get; set; }
            public int DomColor { get; set; }
            public string Emoji { get; set; }
            public List<GodSkinModel> Skins { get; set; }
        }
    }
}
