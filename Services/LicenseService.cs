using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using SmartInspectFor3DInfotech;
using Streamline.Module.Admin.Helpers;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Models;
using Streamline.Module.Admin.ViewModel.License;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TDI.Licensing.Models;

namespace Streamline.Module.Admin.Services
{
    internal class LicenseService : ServiceBase, ILicenseService
    {
        private static readonly HyperArea Logger = HyperLogger.MakeHyperLog(nameof(LicenseService)).High;
        private static readonly HyperArea Detail = HyperLogger.MakeHyperLog(nameof(LicenseService)).Detail;
        private readonly IServiceProvider _serviceProvider;
        private LicenseViewModel _licenseView;





        public LicenseService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }

        public void Initialize()
        {
        }
        public IEnumerable<LicenseTypeViewModel> GetLicenseTypes()
        {
            var types = Enum.GetValues(typeof(LicenseType));
            foreach (var item in types)
            {
                var licenseType = new LicenseTypeViewModel(this._serviceProvider, ((Enum)item).GetDescription());
                if (item.ToString() == LicenseType.Flat.ToString())
                {
                    licenseType.Detail.Add(new ActiveOnlineViewModel(this.ServiceProvider, LicenseType.Flat, $"{CommonStringsAdmin.UIStrings.Active} {CommonStringsAdmin.UIStrings.Online}"));
                    licenseType.Detail.Add(new ActiveManualViewModel(this.ServiceProvider, LicenseType.Flat, $"{CommonStringsAdmin.UIStrings.Active} {CommonStringsAdmin.UIStrings.Manual}"));
                    licenseType.Detail.Add(new TelephonicActivationViewModel(this.ServiceProvider, $"{CommonStringsAdmin.UIStrings.Active} {CommonStringsAdmin.UIStrings.Telephone}"));
                }
                else
                {
                    licenseType.Detail.Add(new ActiveOnlineViewModel(this.ServiceProvider, LicenseType.Network, $"{CommonStringsAdmin.UIStrings.Active} {CommonStringsAdmin.UIStrings.Online}"));
                    licenseType.Detail.Add(new ActiveManualViewModel(this.ServiceProvider, LicenseType.Network, $"{CommonStringsAdmin.UIStrings.Active} {CommonStringsAdmin.UIStrings.Manual}"));
                }
                yield return licenseType;
            }
        }
        public LicenseViewModel GetLicenseViewModel()
        {
            _licenseView = new LicenseViewModel(_serviceProvider);
            return _licenseView;
        }


    }
}
