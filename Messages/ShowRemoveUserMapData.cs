using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowRemoveUserMapData
    {
        public ViewModelBase Data { get; set; }
        public ShowRemoveUserMapData(ViewModelBase view)
        {
            this.Data = view;
        }
    }
}
