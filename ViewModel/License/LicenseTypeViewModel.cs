using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Streamline.Module.Admin.Interfaces;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class LicenseTypeViewModel : ViewModelBase
    {
        private IServiceProvider _serviceProvider;
        private ILicenseService _licenseService;
        [ObservableProperty]
        private string _name;
    

        [ObservableProperty]
        private ObservableCollection<ViewModelBase> _detail = new ObservableCollection<ViewModelBase>();
        public LicenseTypeViewModel(IServiceProvider serviceProvider, string name)
        {
            _serviceProvider = serviceProvider;
            _licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            _licenseService.Initialize();
            _name = name;
        }
    }
}
