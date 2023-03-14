using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowActiveManualLicenseViewMessage : ValueChangedMessage<ShowActiveManualLicenseDetailData>
    {
        public ShowActiveManualLicenseViewMessage(ShowActiveManualLicenseDetailData detailData) : base(detailData)
        {
            if (detailData == null) throw new ArgumentNullException(nameof(detailData));
        }
    }
}
