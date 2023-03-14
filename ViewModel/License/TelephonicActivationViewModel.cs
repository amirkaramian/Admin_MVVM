using Notifications.Wpf.Core;
using Streamline.Module.Admin.Interfaces;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDI.Licensing.Interfaces;
using TDI.Licensing;
using Microsoft.Extensions.DependencyInjection;
using TDI.Licensing.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Streamline.Module.Admin.Messages;
using Streamline.Common.Messaging;
using System.Reflection;
using System.Windows;


namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class TelephonicActivationViewModel : ViewModelBase
    {
        private IServiceProvider _serviceProvider;
        private ISKLicenseManagerService _sKLicenseManager;
        private INotificationManager _notification;
        private ILicenseService _licenseService;
        private ILicenseManager License;
        [ObservableProperty]
        private string _userCode1;
        [ObservableProperty]
        private string _userCode2;
        [ObservableProperty]
        private string _activationCode1;
        [ObservableProperty]
        private string _activationCode2;
        [ObservableProperty]
        private bool _isSelected;
        [ObservableProperty]
        private string _name;
        public RelayCommand SelectCommand { get; private set; }
        private RelayCommand _activeCommand;
        public RelayCommand ActiveCommand
        {
            get
            {
                return _activeCommand ?? new RelayCommand(() =>
                {
                    TelephonicActivation();
                });
            }
        }
        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? new RelayCommand(() =>
                {
                    this.License.ClearSessionCode();
                    this.IsSelected = false;
                    var model = _licenseService.GetLicenseViewModel();
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                });
            }
        }
        private RelayCommand _resumeCommand;
        public RelayCommand ResumeCommand
        {
            get
            {
                return _resumeCommand ?? new RelayCommand(() =>
                {
                    this.IsSelected = false;
                    var model = _licenseService.GetLicenseViewModel();
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                });
            }
        }
        public TelephonicActivationViewModel(IServiceProvider serviceProvider, string name)
        {
            _name = name;
            _serviceProvider = serviceProvider;
            _sKLicenseManager = _serviceProvider.GetRequiredService<ISKLicenseManagerService>();
            _licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            License = _sKLicenseManager.License;
            _notification = _serviceProvider.GetRequiredService<INotificationManager>();
            SelectCommand = new RelayCommand(() =>
            {
                IsSelected = true;
                Broadcaster.SendMessage(new ShowTelephoneActivationLicenseViewMessage(new ShowTelephoneActivationLicenseDetailData(this)));                
            });
        }

        internal async void TelephonicActivation()
        {
            this.License.Log("frmTelephonicActivation.TelephonicActivation(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            try
            {
                var m_ActivationCode1 = 0;
                var m_ActivationCode2 = 0;
                if (!int.TryParse(this.ActivationCode1, out m_ActivationCode1))
                {
                    await _notification.ShowAsync(new NotificationContent { Title = this.License.ProdConfig.ProductName + CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.InvalidActivecode, Type = NotificationType.Warning }, areaName: "WindowArea");
                    return;
                }
                if (!int.TryParse(this.ActivationCode2, out m_ActivationCode2))
                {
                    await _notification.ShowAsync(new NotificationContent { Title = this.License.ProdConfig.ProductName + CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.InvalidActivecode, Type = NotificationType.Warning }, areaName: "WindowArea");
                    return;
                }
                if (!License.TelephonicActivation(m_ActivationCode1, m_ActivationCode2))
                {
                    await _notification.ShowAsync(new NotificationContent { Title = this.License.ProdConfig.ProductName + CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.ActivationError + this.License.ExtendedError, Type = NotificationType.Warning }, areaName: "WindowArea");
                }
                else
                {
                    await _notification.ShowAsync(new NotificationContent { Title = this.License.ProdConfig.ProductName + CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.ActivationLicenseSuccessful, Type = NotificationType.Success }, areaName: "WindowArea");
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(_licenseService.GetLicenseViewModel())));
                }
            }
            catch (Exception exception)
            {
                this.License.Log("Error activating license telephonically with the message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
            }
            finally
            {
                this.License.Log("frmTelephonicActivation.TelephonicActivation(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            }
        }
    }
}
