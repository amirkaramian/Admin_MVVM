using CommunityToolkit.Mvvm.Messaging.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal sealed class ShowUserMapDetailViewMessage : ValueChangedMessage<ShowUserMapDetailData>
    {
        
        public ShowUserMapDetailViewMessage(ShowUserMapDetailData detailData) : base(detailData)
        {
            if (detailData == null) throw new ArgumentNullException(nameof(detailData));
        }
    }
}
