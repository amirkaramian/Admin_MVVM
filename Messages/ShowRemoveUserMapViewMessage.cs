using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowRemoveUserMapViewMessage : ValueChangedMessage<ShowRemoveUserMapData>
    {
        public ShowRemoveUserMapViewMessage(ShowRemoveUserMapData data) : base(data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
        }
    }
}
