using Streamline.Module.Admin.ViewModel.License;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Interfaces
{
    internal interface ILicenseService
    {
        void Initialize();
        IEnumerable<LicenseTypeViewModel> GetLicenseTypes();
        LicenseViewModel GetLicenseViewModel();
    }
}
