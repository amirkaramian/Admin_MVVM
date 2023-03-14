using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Helpers
{
    internal static class Extentions
    {
        public static string GetDescription(this Enum item)
        {
            var type = item.GetType();
            var data = type.GetField(Enum.GetName(type, item)).GetCustomAttributes(false).OfType<DescriptionAttribute>().SingleOrDefault();
            return data == null ? string.Empty : data.Description;
        }
    }
}
