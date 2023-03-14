using com.softwarekey.Client.Licensing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using Streamline.Common.MVVM;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TDI.Licensing;
using TDI.Licensing.Interfaces;
using TDI.Licensing.Models;
using TDI.Licensing.Network;
using Brushes = System.Windows.Media.Brushes;


namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class LicenseViewModel : ViewModelBase
    {
        private IServiceProvider _serviceProvider;
        private ISKLicenseManagerService _sKLicenseManager;
        private INotificationManager _notification;
        private ILicenseManager License;
        private bool _netWorkEnabled => NetworkLicenseConfiguration.GetConfiguration.NetworkEnable;

        [ObservableProperty]
        private string _status;
        [ObservableProperty]
        private string _licenseInfo;
        [ObservableProperty]
        private string _licenseStatus;
        [ObservableProperty]
        private string _userCount;
        [ObservableProperty]
        private string _networkPath;
        [ObservableProperty]
        private ImageSource _headerImageSource;
        [ObservableProperty]
        private SolidColorBrush _colorStatus;
        [ObservableProperty]
        private Visibility _networkPathVisible;
        [ObservableProperty]
        private Visibility _networkCommandVisible;
        [ObservableProperty]
        private Brush _networkPathColor;
        [ObservableProperty]
        private bool _canRefereshCommnadExecuted;
        [ObservableProperty]
        private bool _canDeactiveCommnadExecuted;
        [ObservableProperty]
        private ObservableCollection<Feature> _moduleItems = new ObservableCollection<Feature>();

        private RelayCommand _refereshCommnad;
        public RelayCommand RefereshCommnad
        {
            get
            {
                return _refereshCommnad ?? new RelayCommand(async () =>
                {
                    await RefreshLicenseStatus();

                }, () => _canRefereshCommnadExecuted);
            }
        }

        private RelayCommand _deactiveCommnad;
        public RelayCommand DeactiveCommnad
        {
            get { return _deactiveCommnad ?? new RelayCommand(async () => { await Deactivate(); }, () => _canDeactiveCommnadExecuted); }
        }
        public LicenseViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _sKLicenseManager = _serviceProvider.GetRequiredService<ISKLicenseManagerService>();
            _notification = serviceProvider.GetRequiredService<INotificationManager>();
            try
            {
                _headerImageSource = new BitmapImage(new Uri($"pack://application:,,,{_sKLicenseManager.LicenseConfiguration.Logo}"));
            }
            catch { }
            CanDeactiveCommnadExecuted = false;
            CanRefereshCommnadExecuted = false;
            License = _sKLicenseManager.License;
            UpdateLicenseInformation();
            }

            private void UpdateLicenseInformation()
            {
                License.Log("frmMainLicense.UpdateLicenseInformation(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                try
                {
                    Status = LicenseInfo = LicenseStatus = UserCount = string.Empty;
                    NetworkPathVisible = !_netWorkEnabled ? Visibility.Hidden : Visibility.Visible;
                    NetworkCommandVisible = !_netWorkEnabled ? Visibility.Hidden : Visibility.Visible;
                    NetworkPath = _netWorkEnabled ? $"NetworkPath: {NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue}" : string.Empty;
                    if (_netWorkEnabled)
                        ReloadNetworkLicense();
                    else
                        ReloadFlatLicense();
                    LicenseStatus status = License.Status;
                    CanDeactiveCommnadExecuted = status == TDI.Licensing.Models.LicenseStatus.LicenseReady;

                    CanRefereshCommnadExecuted = status == TDI.Licensing.Models.LicenseStatus.LicenseReady ||
                                                    status == TDI.Licensing.Models.LicenseStatus.LicenseInvalidatedFromServer
                                                        && !_sKLicenseManager.License.isTelephonicActivation;
                    UpdateModuleInformation();
                }
                catch (Exception ex)
                {
                    License.Log("Error updating license information with the error message: " + ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                }

                License.Log("frmMainLicense.UpdateLicenseInformation(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            }
            private void ReloadNetworkLicense()
            {
                try
                {

                    if (!_sKLicenseManager.License.LoadFile(NetworkLicenseConfiguration.GetConfiguration.LicenseFilePath))
                    {
                        CanRefereshCommnadExecuted = false;
                        CanDeactiveCommnadExecuted = false;

                        if (_sKLicenseManager.License.LastError.ErrorNumber == LicenseError.ERROR_PLUS_EVALUATION_INVALID)
                        {
                            //Invalid Protection PLUS 5 SDK evaluation envelope.
                            Status = _sKLicenseManager.License.LastError.ErrorString;
                        }
                        else
                        {
                            Status = _sKLicenseManager.License.GenerateLicenseErrorString();
                            ColorStatus = Brushes.DarkRed;
                            LicenseInfo = string.Empty;
                        }

                        UserCount = "N/A";

                        //if (License.m_Semaphore != null)
                        //{
                        //    License.m_Semaphore.Close();
                        //    License.m_Semaphore = null;
                        //}

                        return;
                    }

                    CanRefereshCommnadExecuted = true;
                    CanDeactiveCommnadExecuted = true;

                    RefreshNetworkLicenseStatus();
                }
                catch (Exception ex)
                {
                    License.Log(ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    throw ex;
                }

            }
            private void RefreshNetworkLicenseStatus()
            {
                try
                {
                    CanRefereshCommnadExecuted = License.InstallationID.Length > 0;
                    bool isLicenseValid = License.Validate();
                    if (!isLicenseValid)
                    {
                        //if (License.m_Semaphore != null)
                        //{
                        //    License.m_Semaphore.Close(); // close our network session if it is open
                        //    License.m_Semaphore = null;
                        //}

                        Status = License.GenerateLicenseStatusEntry(isLicenseValid);
                        ColorStatus = System.Windows.Media.Brushes.DarkRed;
                        LicenseInfo = string.Empty;
                        LicenseInfo = $"[{CommonStringsAdmin.UIStrings.LicenseStatus}]";
                        UserCount = "N/A";

                    }
                    else
                    {
                        //if (License.m_Semaphore == null)
                        //{
                        //    var directory = Path.GetDirectoryName(NetworkLicenseConfiguration.GetConfiguration.LicenseFilePath);
                        //    var prefix = NetworkLicenseConfiguration.GetConfiguration.NetworkSemaphorePrefix;
                        //    License.m_Semaphore = new NetworkSemaphore(directory, prefix, License.LicenseCounter, true, 15, true);
                        //    License.m_Semaphore.Invalid += new NetworkSemaphore.InvalidEventHandler(InvalidSemaphoreHandler);

                        //    if (!License.m_Semaphore.Open() && License.m_Semaphore.LastError.ErrorNumber == LicenseError.ERROR_NETWORK_LICENSE_FULL) // try to open a network session
                        //    {
                        //        //using (frmNetworkLicenseSearch searchDlg = new frmNetworkLicenseSearch(License.m_Semaphore)) // try to search for an open network seat
                        //        {
                        //            //if (searchDlg.ShowDialog() != DialogResult.OK)
                        //            {
                        //                _status = License.GenerateLicenseStatusEntry(isLicenseValid);
                        //                _userCount = "No Seats Available.";
                        //                License.m_Semaphore = null;
                        //            }
                        //        }
                        //    }
                        //    else if (License.m_Semaphore.LastError.ErrorNumber != LicenseError.ERROR_NONE)
                        //    {
                        //        _status = "Unable to establish a network session. " + License.m_Semaphore.LastError;
                        //        _colorStatus = Brushes.DarkRed;
                        //        _userCount = "N/A";
                        //        License.m_Semaphore = null;
                        //    }
                        //}

                        //if (License.m_Semaphore != null && License.m_Semaphore.IsValid)
                        {
                            var resp = License.GenerateLicenseStatusEntry(isLicenseValid);
                            var resps = resp.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            Status = resps[0];
                            ColorStatus = Brushes.DarkGreen;
                            LicenseInfo = resps[1];
                            LicenseStatus = string.Empty;
                            if (resps.Length > 2)
                                for (var i = 2; i < resps.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(resps[i]))
                                    {
                                        LicenseStatus += Environment.NewLine;
                                        continue;
                                    }
                                    LicenseStatus += resps[i] + Environment.NewLine + Environment.NewLine;
                                }
                            // UserCount = License.m_Semaphore.SeatsActive.ToString() + " out of " + License.LicenseCounter.ToString(); // display how many network users are running the application
                            if (!NetworkLicenseConfiguration.GetConfiguration.NetworkEnable)
                                NetworkLicenseConfiguration.GetConfiguration.NetworkEnable = true;
                            //MessageBox.Show("Connect to network Successfully", "Network floating", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    License.Log(ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    throw ex;
                }

            }
            private void ReloadFlatLicense()
            {
                switch (License.Status)
                {
                    case TDI.Licensing.Models.LicenseStatus.LicenseNoLicensing:
                    case TDI.Licensing.Models.LicenseStatus.None:
                        ColorStatus = Brushes.DarkRed;
                        Status = CommonStringsAdmin.UIStrings.NotLicensed;
                        LicenseStatus = CommonStringsAdmin.UIStrings.LicenseNotFound;
                        break;
                    case TDI.Licensing.Models.LicenseStatus.LicenseReady:
                        ColorStatus = Brushes.DarkGreen;
                        Status = CommonStringsAdmin.UIStrings.OkLabel;

                        if (License.isTelephonicActivation)
                            LicenseStatus = CommonStringsAdmin.UIStrings.ActivatedAsPerpetual;
                        else
                            LicenseStatus = GetLicenseInfoStr();

                        break;
                    default:
                        ColorStatus = Brushes.DarkRed;
                        Status = CommonStringsAdmin.UIStrings.NotLicensed;
                        break;
                }

                if (License.LastError.ErrorNumber != 0 && string.IsNullOrEmpty(LicenseStatus))
                    LicenseStatus = License.GenerateLicenseErrorString();
            }
            private void UpdateModuleInformation()
            {
                _moduleItems.Clear();
                if (this.License.ProductFeatures?.ListFeatures.Count > 0)
                {
                    for (int i = 0; i < this.License.ProdConfig.OptionalModules.Count; i++)
                    {
                        Feature feature = this.License.ProductFeatures.ListFeatures[this.License.ProdConfig.OptionalModules[i]];
                        _moduleItems.Add(feature);
                    }
                }
            }
            private string GetLicenseInfoStr()
            {
                License.Log("frmMainLicense.GetLicenseInfoStr(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);

                StringBuilder registerInfo = new StringBuilder();

                try
                {
                    //Check if first name is not empty and not unregistered
                    if (!string.IsNullOrEmpty(License.Customer.FirstName) && License.Customer.FirstName != "UNREGISTERED")
                    {
                        //
                        registerInfo.Append($"{CommonStringsAdmin.UIStrings.RegisteredTo}: ");

                        //Append first name
                        registerInfo.Append(License.Customer.FirstName);
                    }

                    //Check if last name is not empty and not unregistered
                    if (License.Customer.LastName != "" && License.Customer.LastName != "UNREGISTERED")
                    {
                        if (registerInfo.ToString() == "")
                            registerInfo.Append($"{CommonStringsAdmin.UIStrings.RegisteredTo}: ");

                        registerInfo.Append(" ");

                        //Append last name
                        registerInfo.Append(License.Customer.LastName);
                    }

                    //Check if company name is not empty and not unregistered
                    if (!string.IsNullOrEmpty(License.Customer.CompanyName) && License.Customer.CompanyName != "UNREGISTERED")
                    {
                        if (registerInfo.ToString() == "")
                            registerInfo.Append($"{CommonStringsAdmin.UIStrings.RegisteredTo}: ");

                        registerInfo.Append(" ");

                        //Append company name
                        registerInfo.AppendLine("[" + License.Customer.CompanyName + "]");
                    }

                    // 
                    if (registerInfo.ToString() != "")
                        registerInfo.Append(Environment.NewLine);

                    //Append license ID
                    registerInfo.AppendLine($"{CommonStringsAdmin.UIStrings.LicenseId}: " + License.InternalSoftwareKeyLicense.LicenseID);

                    //If the application has been activated with a test license then show a warning.
                    if (License.InternalSoftwareKeyLicense.IsTestLicense)
                    {
                        string productName = string.Empty;
                        if (License.ProdConfig != null)
                        {
                            productName = License.ProdConfig.ProductName;
                        }
                        else
                        {
                            productName = $"[{CommonStringsAdmin.UIStrings.ProductName}]";
                            License.Log("ProductLicenseConfiguration not assigned to LicenseManager.", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                        }

                        registerInfo.AppendLine();
                        registerInfo.AppendLine(productName + $" {CommonStringsAdmin.UIStrings.LicenseInfo1}");
                    }

                    if (License.Status == TDI.Licensing.Models.LicenseStatus.LicenseReady)
                    {
                        registerInfo.AppendLine();
                        registerInfo.AppendLine(License.GenerateLicenseStatusEntry());
                    }
                }
                catch (Exception ex)
                {
                    License.Log("Error getting license information with the error message: " + ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                }

                License.Log("frmMainLicense.GetLicenseInfoStr(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                return registerInfo.ToString();
            }
            private async Task Deactivate()
            {
                License.Log("frmMainLicense.btnDeactivate_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                CanDeactiveCommnadExecuted = false;
                CanRefereshCommnadExecuted = false;
                try
                {
                    if (License.isTelephonicActivation && !License.IsRunningAsAdministrator())
                    {
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Deactivation, Message = CommonStringsAdmin.UIStrings.MessageInfo1, Type = NotificationType.Success }, areaName: "WindowArea");
                        UpdateLicenseInformation();
                        return;
                    }
                    if (MessageBox.Show(string.Format(CommonStringsAdmin.UIStrings.DeactivationConfirm, License.ProdConfig.ProductName), License.ProdConfig.ProductName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (License.isTelephonicActivation && License.DeactivateTelephonicLicense())
                            await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Deactivation, Message = CommonStringsAdmin.UIStrings.DeactivationSuccessful, Type = NotificationType.Success }, areaName: "WindowArea");
                        else if (!License.isTelephonicActivation && License.DeactivateOnline())
                            await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Deactivation, Message = CommonStringsAdmin.UIStrings.DeactivationSuccessful + License.ProdConfig.ProductName + CommonStringsAdmin.UIStrings.Deactivation, Type = NotificationType.Success }, areaName: "WindowArea");
                        else
                        {
                            StringBuilder msg = new StringBuilder();
                            msg.AppendLine("The license was not deactivated.");
                            msg.AppendFormat("Error: ({0}) {1}", License.LastError.ErrorNumber, License.LastError.ErrorString);
                            if (License.LastError.ExtendedErrorNumber > 0)
                            {
                                msg.AppendLine();
                                msg.AppendFormat("{0}.", LicenseError.GetWebServiceErrorMessage(License.LastError.ExtendedErrorNumber));
                            }
                            await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Deactivation, Message = msg.ToString(), Type = NotificationType.Warning }, areaName: "WindowArea");
                        }

                        UpdateLicenseInformation();
                    }
                }
                catch (Exception ex)
                {
                    License.Log("Error deactivating license with the error message: " + ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                }

                License.Log("frmMainLicense.btnDeactivate_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            }
            private async Task RefreshLicenseStatus()
            {
                License.Log("frmMainLicense.btnRefresh_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                CanRefereshCommnadExecuted = false;
                CanDeactiveCommnadExecuted = false;
                try
                {
                    if (License.RefreshLicense())
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseRefresh, Message = string.Format(CommonStringsAdmin.UIStrings.RefreshSuccessful, License.ProdConfig.ProductName), Type = NotificationType.Success }, areaName: "WindowArea");
                    else
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine(CommonStringsAdmin.UIStrings.LicenseInfo2);
                        msg.AppendFormat($"{CommonStringsAdmin.UIStrings.Error}: ({0}) {1}, {2}", License.LastError.ErrorNumber, License.LastError.ErrorString, License.ProdConfig.ProductName);
                        if (License.LastError.ExtendedErrorNumber > 0)
                        {
                            msg.AppendLine();
                            msg.AppendFormat("{0}.", LicenseError.GetWebServiceErrorMessage(License.LastError.ExtendedErrorNumber));
                        }
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.LicenseRefresh, Message = msg.ToString(), Type = NotificationType.Success }, areaName: "WindowArea");
                    }
                }
                catch (Exception ex)
                {
                    License.Log("Error refreshing license with the error message: " + ex.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                }
                finally
                {
                    CanRefereshCommnadExecuted = true;
                    CanDeactiveCommnadExecuted = true;
                    License.Log("frmMainLicense.btnRefresh_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                }

            }
        }
    }
