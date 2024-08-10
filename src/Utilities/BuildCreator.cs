using System.Linq;
using System.Text;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
    public class BuildCreator
    {
        public static string CreateBuild(string itemString)
        {
            StringBuilder sb = new();
            var splitItemString = itemString.Split(',');
            for (int i = 0; i < splitItemString.Length; i++)
            {
                var trimmed = splitItemString[i].Trim();
                var itemEmoji = FindItem(trimmed);
                if (itemEmoji.Length != 0)
                {
                    sb.Append(itemEmoji);
                }
            }
            return sb.ToString();
        }
        private static string FindItem(string itemName)
        {
            string result = "";
            var items = Constants.ItemsHashSet.ToList();
            if (items.Any(x => x.DeviceName.ToLowerInvariant().Contains(itemName.ToLowerInvariant())))
            {
                var foundItem = items.Find(x => x.DeviceName.ToLowerInvariant().Contains(itemName.ToLowerInvariant()));
                return foundItem.Emoji;
            }
            return result;
        }
    }
}
