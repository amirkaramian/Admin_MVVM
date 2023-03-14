using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Messages;
using Streamline.Common.Messaging;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class LicenseItemsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILicenseService _licenseService;
        [ObservableProperty]
        private ObservableCollection<LicenseTypeViewModel> _items = new ObservableCollection<LicenseTypeViewModel>();
        [ObservableProperty]
        private LicenseViewModel _model;
        [ObservableProperty]
        private PackIconControlBase _icon = new PackIconUnicons { Kind = PackIconUniconsKind.Unlock };
        private RelayCommand _selectCommand;
        public RelayCommand SelectCommand
        {
            get
            {
                return _selectCommand ?? new RelayCommand(() =>
                {
                    _model = _licenseService.GetLicenseViewModel();
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(_model)));
                });
            }
        }

        [ObservableProperty]
        private PackIconControlBase _iconSmall = new PackIconUnicons
        {
            Kind = PackIconUniconsKind.LockAccess,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Height = 32,
            Width = 32
        };

        public LicenseItemsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            foreach (var item in _licenseService.GetLicenseTypes())
                _items.Add(item);
            StrongReferenceMessenger.Default.Register<ShowActiveOnlineLicenseViewMessage>(this, ShowActiveOnlineView);
            StrongReferenceMessenger.Default.Register<ShowActiveManualLicenseViewMessage>(this, ShowActiveManualView);
            StrongReferenceMessenger.Default.Register<ShowTelephoneActivationLicenseViewMessage>(this, ShowTelephoneActivationView);
        }
        private void ShowTelephoneActivationView(object recipient, ShowTelephoneActivationLicenseViewMessage message)
        {
            Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(message.Value.Data)));
        }

        private void ShowActiveManualView(object recipient, ShowActiveManualLicenseViewMessage message)
        {
            Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(message.Value.Data)));
        }

        private void ShowActiveOnlineView(object recipient, ShowActiveOnlineLicenseViewMessage message)
        {
            Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(message.Value.Data)));
        }
    }
}
