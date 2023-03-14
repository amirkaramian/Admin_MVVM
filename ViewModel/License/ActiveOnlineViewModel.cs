using com.softwarekey.Client.Licensing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Messages;
using Streamline.Module.Admin.Views.License;
using Streamline.Common.Messaging;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TDI.Licensing;
using TDI.Licensing.Interfaces;
using TDI.Licensing.Models;
using TDI.Licensing.Network;

namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class ActiveOnlineViewModel : ViewModelMaster
    {
        private IServiceProvider _serviceProvider;
        private ISKLicenseManagerService _sKLicenseManager;
        private INotificationManager _notification;
        private ILicenseService _licenseService;
        private ILicenseManager License;
        private LicenseType _licenseType;
        private bool isNetworkMode => _licenseType == LicenseType.Network;
        [ObservableProperty]
        private string _labelInfo;
        [ObservableProperty]
        private string _licenseId;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _installationName;
        [ObservableProperty]
        private bool _canActivationExecute;
        [ObservableProperty]
        private bool _isSelected;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _networkPath;
        [ObservableProperty]
        private SolidColorBrush _networkPathColor;
        [ObservableProperty]
        private Visibility _networkVisible;
        [ObservableProperty]
        private bool _networkChecked;
        public RelayCommand SelectCommand { get; private set; }

        private RelayCommand _activationCommand;
        public RelayCommand ActivationCommand
        {
            get
            {
                return _activationCommand ?? new RelayCommand(async () =>
                {
                    if (base.HasErrors)
                    {
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = Error, Type = NotificationType.Warning }, areaName: "WindowArea");
                        return;
                    }
                    Activation();
                }, () => _canActivationExecute);
            }
        }
        
        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? new RelayCommand(() =>
                {
                    this.IsSelected = false;
                    License.OnStatusChange -= License_OnStatusChange;
                    var model = _licenseService.GetLicenseViewModel();
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                });
            }
        }

        private RelayCommand _proxySettingCommand;
        public RelayCommand ProxySettingCommand
        {
            get
            {
                return _proxySettingCommand ?? new RelayCommand(() =>
                {
                    string m_ProxyAddress = "";
                    var web = new WebServiceHelper();
                    //if (string.IsNullOrEmpty(m_ProxyAddress))
                    //{
                    //    m_ProxyAddress = web.m_ConnectionInformation.ProxyAddress;
                    //}
                    var win = new ProxyCredentialsView();
                    ProxyCredentialsViewModel proxyCredentialsViewModel = new ProxyCredentialsViewModel(win, m_ProxyAddress);


                    win.DataContext = proxyCredentialsViewModel;
                    win.ShowDialog();

                    if ((bool)win.DialogResult)
                    {
                        web.m_ProxyAuthenticationCredentials = proxyCredentialsViewModel.Credentials;
                        m_ProxyAddress = proxyCredentialsViewModel.ProxyAddress;
                    }
                });
            }
        }

        private RelayCommand<Object> _passwordChangedCommand;
        public RelayCommand<Object> PasswordChangedCommand
        {
            get
            {
                return _passwordChangedCommand ?? new RelayCommand<Object>(obj =>
                {
                    if (obj == null) return;
                    _password = ((System.Windows.Controls.PasswordBox)obj).Password;
                });
            }
        }
        public ActiveOnlineViewModel(IServiceProvider serviceProvider, LicenseType licenseType, string name)
        {
            _serviceProvider = serviceProvider;
            _licenseType = licenseType;
            _name = name;
            _labelInfo = CommonStringsAdmin.UIStrings.LicenseActivationInfo;
            InitializeView();
            SelectCommand = new RelayCommand(() =>
            {
                IsSelected = true;
                ReloadViewModel();
                Broadcaster.SendMessage(new ShowActiveOnlineLicenseViewMessage(new ShowActiveOnlineLicenseDetailData(this)));
            });
        }
        private async void Activation()
        {
            try
            {
                LoadNetworkPathAsync();
                CanActivationExecute = false;
                License.InstallationName = _installationName;
                if (License.ActivateOnline(int.Parse(_licenseId), _password))
                {
                    if (License.Status == LicenseStatus.LicenseReady)
                    {
                        NetworkLicenseConfiguration.GetConfiguration.NetworkEnable = _licenseType == LicenseType.Network;
                        License.OnStatusChange -= License_OnStatusChange;
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.ActivationLicenseSuccessful, Type = NotificationType.Success }, areaName: "WindowArea");                        
                        var model = _licenseService.GetLicenseViewModel();
                        Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                    }
                    else
                    {

                        StringBuilder msg = new StringBuilder();
                        msg.AppendLine(CommonStringsAdmin.UIStrings.ActivationFailed);
                        msg.AppendFormat($"{CommonStringsAdmin.UIStrings.Error}: ({0}) {1}", License.LastError.ErrorNumber, License.LastError.ErrorString);
                        if (License.LastError.ExtendedErrorNumber > 0)
                        {
                            msg.AppendLine();
                            msg.AppendFormat("{0}.", LicenseError.GetWebServiceErrorMessage(License.LastError.ExtendedErrorNumber));
                        }
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = msg.ToString(), Type = NotificationType.Warning }, areaName: "WindowArea");

                    }

                }
                else
                {

                    StringBuilder msg = new StringBuilder();
                    msg.AppendLine(CommonStringsAdmin.UIStrings.ActivationFailed);
                    msg.AppendFormat($"{CommonStringsAdmin.UIStrings.Error}: ({0}) {1}", License.LastError.ErrorNumber, License.LastError.ErrorString);
                    if (License.LastError.ExtendedErrorNumber > 0)
                    {
                        msg.AppendLine();
                        msg.AppendFormat("{0}.", LicenseError.GetWebServiceErrorMessage(License.LastError.ExtendedErrorNumber));
                    }
                    await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = msg.ToString(), Type = NotificationType.Warning }, areaName: "WindowArea");

                }
            }
            catch (Exception ex)
            {
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = ex.Message, Type = NotificationType.Error }, areaName: "WindowArea");
            }
            finally
            {
                CanActivationExecute = true;
            }
        }
        private void LoadNetworkPathAsync()
        {
            if (this._licenseType == LicenseType.Flat)
            {
                _sKLicenseManager.LicenseType = LicenseType.Flat;
                License = _sKLicenseManager.License;
                return;
            }
            if (NetworkChecked && ValidateNetworkPath())
            {
                CreateNetwork();
                return;
            }
            System.Windows.Forms.FolderBrowserDialog fbDlg = new System.Windows.Forms.FolderBrowserDialog();
            if (fbDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NetworkPath = fbDlg.SelectedPath;
                if (!ValidateNetworkPath())
                    throw new Exception("Invalid network path");
                CreateNetwork();
            }
            else
            {
                throw new Exception("Network path not selected");
            }
        }
        private async void License_OnStatusChange(object? sender, EventArgs e)
        {
            var message = string.Empty;
            if (!License.LoadFile(NetworkLicenseConfiguration.GetConfiguration.LicenseFilePath))
            {
                if (License.LastError.ErrorNumber == LicenseError.ERROR_PLUS_EVALUATION_INVALID)
                {
                    //Invalid Protection PLUS 5 SDK evaluation envelope.
                    message = License.LastError.ErrorString;
                }
                else
                {
                    message = License.GenerateLicenseErrorString();
                }
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = message, Type = NotificationType.Error }, areaName: "WindowArea");
                return;
            }

            bool isLicenseValid = License.Validate();
            if (!isLicenseValid)
            {
                message = License.GenerateLicenseStatusEntry(isLicenseValid);
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = message, Type = NotificationType.Error }, areaName: "WindowArea");
            }
            else
            {
                License.GenerateLicenseStatusEntry(isLicenseValid);
                NetworkLicenseConfiguration.GetConfiguration.NetworkEnable = true;
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseManager, Message = CommonStringsAdmin.UIStrings.ConnectNetworkSuccessfully, Type = NotificationType.Success }, areaName: "WindowArea");

            }
        }

        private void CreateNetwork()
        {
            NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue = NetworkPath;
            NetworkLicenseConfiguration.GetConfiguration.LicenseFilePath = NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue;
            License?.Dispose();
            License = null;
            _sKLicenseManager.LicenseType = LicenseType.Network;
            License = _sKLicenseManager.License;
            License.OnStatusChange += License_OnStatusChange;
        }
        private bool ValidateNetworkPath()
        {
            if (string.IsNullOrEmpty(NetworkPath))
                return false;

            var path = FileUtils.EnsureUNCPath(NetworkPath);
            if (path.StartsWith(@"\\") && Directory.Exists(path))
            {
                NetworkPath = path;
                NetworkPathColor = Brushes.Green;
                return true;
            }
            else
            {
                NetworkPathColor = Brushes.Red;
                return false;
            }
        }
        private void InitializeView()
        {
            base.AddRule(() => LicenseId, () =>
            !string.IsNullOrEmpty(LicenseId) &&
            LicenseId.Length > 1 && int.TryParse(LicenseId, out int result) &&
            LicenseId.Length < 100, CommonStringsAdmin.UIStrings.LicenseIdValidation);

            base.AddRule(() => Password, () =>
            !string.IsNullOrEmpty(Password), CommonStringsAdmin.UIStrings.PasswordRequierd);

            _sKLicenseManager = _serviceProvider.GetRequiredService<ISKLicenseManagerService>();
            _licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            _notification = _serviceProvider.GetRequiredService<INotificationManager>();

        }
        private void ReloadViewModel()
        {
            License = _sKLicenseManager.License;

            CanActivationExecute = License.Status != LicenseStatus.LicenseReady;
            NetworkVisible = !isNetworkMode ? Visibility.Hidden : Visibility.Visible;
            NetworkPath = NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue;
            NetworkPathColor = Brushes.Green;
        }

    }
}
