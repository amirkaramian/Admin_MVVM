﻿using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Messages
{
    internal class ShowActiveManualLicenseDetailData
    {
        public ViewModelBase Data { get; set; }

        public ShowActiveManualLicenseDetailData(ViewModelBase data)
        {
            Data = data;
        }
    }
}
