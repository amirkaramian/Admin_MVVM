using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowAddUserMapViewMessage : ValueChangedMessage<ShowAddUserMapData>
    {
        public ShowAddUserMapViewMessage(ShowAddUserMapData data) : base(data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
        }
    }
}
