using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowNewUserMapViewMessage : ValueChangedMessage<ShowNewUserMapData>
    {
        public ShowNewUserMapViewMessage(ShowNewUserMapData value) : base(value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
        }
    }
}
