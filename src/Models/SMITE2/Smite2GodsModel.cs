using System;

namespace ThothBotCore.Models.SMITE2
{
    public class Smite2GodsModel
    {
        public Pageprops pageProps { get; set; }
        public bool __N_SSG { get; set; }
        public God[] data { get; set; }

        public class Pageprops
        {
            public God[] gods { get; set; }
            public MainRoles[] roles { get; set; }
            public MainPantheon[] pantheons { get; set; }
            public object difficulties { get; set; }
        }

        public class God
        {
            public int id { get; set; }
            public GodAttributes attributes { get; set; }
        }

        public class GodAttributes
        {
            public string Name { get; set; }
            public bool isNew { get; set; }
            public string slug { get; set; }
            public string Subtitle { get; set; }
            public string Lore { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime publishedAt { get; set; }
            public string locale { get; set; }
            public object LoreYouTubeLink { get; set; }
            public Ability[] Ability { get; set; }
            public Skin[] Skin { get; set; }
            public Pantheon pantheon { get; set; }
            public Roles roles { get; set; }
            public Headerimage HeaderImage { get; set; }
            public Type type { get; set; }
            public Portrait Portrait { get; set; }
        }

        public class Pantheon
        {
            public PantheonData data { get; set; }
        }

        public class PantheonData
        {
            public int id { get; set; }
            public PantheonDataAttributes attributes { get; set; }
        }

        public class PantheonDataAttributes
        {
            public string Name { get; set; }
            public string locale { get; set; }
            public PantheonDataAttributesImage Image { get; set; }
        }

        public class PantheonDataAttributesImage
        {
            public PantheonDataAttributesImageData data { get; set; }
        }

        public class PantheonDataAttributesImageData
        {
            public int id { get; set; }
            public PantheonDataAttributesImageDataAttributes attributes { get; set; }
        }

        public class PantheonDataAttributesImageDataAttributes
        {
            public string url { get; set; }
        }

        public class Formats
        {
            public Thumbnail thumbnail { get; set; }
        }

        public class Thumbnail
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Roles
        {
            public RolesData[] data { get; set; }
        }

        public class RolesData
        {
            public int id { get; set; }
            public RolesDataAttributes attributes { get; set; }
        }

        public class RolesDataAttributes
        {
            public string Name { get; set; }
            public string locale { get; set; }
            public RolesDataAttributesImage Image { get; set; }
        }

        public class RolesDataAttributesImage
        {
            public RolesDataAttributesImageData data { get; set; }
        }

        public class RolesDataAttributesImageData
        {
            public int id { get; set; }
            public RolesDataAttributesImageDataAttributes attributes { get; set; }
        }

        public class RolesDataAttributesImageDataAttributes
        {
            public string ext { get; set; }
            public string url { get; set; }
        }

        public class Formats1
        {
            public Thumbnail1 thumbnail { get; set; }
        }

        public class Thumbnail1
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Headerimage
        {
            public HeaderImageData data { get; set; }
        }

        public class HeaderImageData
        {
            public int id { get; set; }
            public HeaderImageDataAttributes attributes { get; set; }
        }

        public class HeaderImageDataAttributes
        {
            public string ext { get; set; }
            public string url { get; set; }
        }

        public class Formats2
        {
            public Thumbnail2 thumbnail { get; set; }
            public Small small { get; set; }
            public Large large { get; set; }
            public Medium medium { get; set; }
        }

        public class Thumbnail2
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Small
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Large
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Medium
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Type
        {
            public object data { get; set; }
        }

        public class Portrait
        {
            public PortraitData data { get; set; }
        }

        public class PortraitData
        {
            public int id { get; set; }
            public PortraitDataAttributes attributes { get; set; }
        }

        public class PortraitDataAttributes
        {
            public string ext { get; set; }
            public string url { get; set; }
        }

        public class Ability
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string Slot { get; set; }
            public string Description { get; set; }
            public string YouTubeLink { get; set; }
            public object[] Buffs { get; set; }
            public AbilityIcon Icon { get; set; }
        }

        public class AbilityIcon
        {
            public AbilityIconData data { get; set; }
        }

        public class AbilityIconData
        {
            public int id { get; set; }
            public AbilityIconDataAttributes attributes { get; set; }
        }

        public class AbilityIconDataAttributes
        {
            public string name { get; set; }
            public string ext { get; set; }
            public string url { get; set; }
            public object previewUrl { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        public class Skin
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string Class { get; set; }
            public SkinImage Image { get; set; }
        }

        public class SkinImage
        {
            public SkinImageData data { get; set; }
        }

        public class SkinImageData
        {
            public int id { get; set; }
            public SkinImageDataAttributes attributes { get; set; }
        }

        public class SkinImageDataAttributes
        {
            public ImgFormats formats { get; set; }
            public string ext { get; set; }
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public object previewUrl { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        public class ImageFormats
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public ImgFormats formats { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class MainRoles
        {
            public int id { get; set; }
            public MainRolesAttributes attributes { get; set; }
        }

        public class MainRolesAttributes
        {
            public string Name { get; set; }
            public Image3 Image { get; set; }
            public Gods gods { get; set; }
            public Localizations localizations { get; set; }
        }

        public class Image3
        {
            public Data data { get; set; }
        }

        public class ImgFormats
        {
            public FormatsUnderFormats thumbnail { get; set; }
            public FormatsUnderFormats small { get; set; }
            public FormatsUnderFormats medium { get; set; }
            public FormatsUnderFormats large { get; set; }
        }

        public class FormatsUnderFormats
        {
            public string name { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }

        public class Gods
        {
            public Datum1[] data { get; set; }
        }

        public class Datum1
        {
            public int id { get; set; }
            public Attributes11 attributes { get; set; }
        }

        public class Attributes11
        {
            public string Name { get; set; }
            public bool isNew { get; set; }
            public string slug { get; set; }
            public string Subtitle { get; set; }
            public string Lore { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime publishedAt { get; set; }
            public string locale { get; set; }
            public object LoreYouTubeLink { get; set; }
        }

        public class Localizations
        {
            public object[] data { get; set; }
        }

        public class MainPantheon
        {
            public int id { get; set; }
            public MainPantheonAttributes attributes { get; set; }
        }

        public class MainPantheonAttributes
        {
            public string Name { get; set; }
            public string locale { get; set; }
            public MainPantheonAttributesImage Image { get; set; }
            public Localizations1 localizations { get; set; }
        }

        public class MainPantheonAttributesImage
        {
            public MainPantheonAttributesImageData data { get; set; }
        }

        public class MainPantheonAttributesImageData
        {
            public int id { get; set; }
            public MainPantheonAttributesImageDataAttributes attributes { get; set; }
        }

        public class MainPantheonAttributesImageDataAttributes
        {
            public string ext { get; set; }
            public string url { get; set; }
        }

        public class Localizations1
        {
            public object[] data { get; set; }
        }

    }
}
