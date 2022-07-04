namespace ThothBotCore.Models
{
    public class GoogleVisionAPIResponseModel
    {
        public Respons[] responses { get; set; }
        public class Respons
        {
            public Imagepropertiesannotation imagePropertiesAnnotation { get; set; }
        }
        public class Imagepropertiesannotation
        {
            public DominantColors dominantColors { get; set; }
        }
        public class DominantColors
        {
            public ColorInfo[] colors { get; set; }
        }
        public class ColorInfo
        {
            public Color color { get; set; }
            public float score { get; set; }
            public float pixelFraction { get; set; }
        }
        public class Color
        {
            public int red { get; set; }
            public int green { get; set; }
            public int blue { get; set; }
        }
    }
}
