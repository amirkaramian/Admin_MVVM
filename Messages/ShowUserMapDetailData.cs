using Streamline.Module.Admin.ViewModel;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Streamline.Module.Admin.Messages.ShowUserMapDetailViewMessage;

namespace Streamline.Module.Admin.Messages
{
    internal sealed class ShowUserMapDetailData
    {
        public ViewModelBase Data { get; private set; }
        public ShowUserMapDetailData(ViewModelBase model)
        {          
            Data = model;           
        }
    }
}
