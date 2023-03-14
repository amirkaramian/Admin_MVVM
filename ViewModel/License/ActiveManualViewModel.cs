using CommunityToolkit.Mvvm.Input;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using TDI.Licensing.Models;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using Streamline.Module.Admin.Interfaces;
using TDI.Licensing.Interfaces;
using TDI.Licensing;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using Streamline.Module.Admin.Messages;
using Streamline.Common.Messaging;
using System.Windows.Media;
using com.softwarekey.Client.Licensing;
using TDI.Licensing.Network;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Logging;

namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class ActiveManualViewModel : ViewModelMaster
    {
        private IServiceProvider _serviceProvider;
        private ISKLicenseManagerService _sKLicenseManager;
        private INotificationManager _notification;
        private ILicenseService _licenseService;
        private LicenseType _licenseType;
        private ILicenseManager License;
        private SaveFileDialog saveFileDialog;
        private OpenFileDialog openFileDialog;
        private bool activationResult;
        private string pathFileOne;
        private string pathFileTwo;
        private string m_sessionCode;
        private bool isNetworkMode => _licenseType == LicenseType.Network;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _licenseId;
        [ObservableProperty]
        private string _activationRequestContent;
        [ObservableProperty]
        private string _activationContent;

        [ObservableProperty]
        private bool _copyCommandCanExceute;
        [ObservableProperty]
        private bool _pasteEnabled;
        [ObservableProperty]
        private bool _saveActivationEnabled;
        [ObservableProperty]
        private bool _loadActivationEnabled;
        [ObservableProperty]
        private bool _generateRequestCommandCanExecute;
        [ObservableProperty]
        private bool _activationCommandEnabled;
        [ObservableProperty]
        private bool _isSelected;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _networkPath;
        [ObservableProperty]
        private Brush _networkPathColor;
        [ObservableProperty]
        private Visibility _networkVisible;
        [ObservableProperty]
        private bool _networkChecked;

        public RelayCommand SelectCommand { get; private set; }

        private RelayCommand _generateRequestCommand;
        public RelayCommand GenerateRequestCommand
        {
            get
            {
                return _generateRequestCommand ?? (_generateRequestCommand = new RelayCommand(async () =>
                {
                    if (base.HasErrors)
                    {
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = Error, Type = NotificationType.Warning }, areaName: "WindowArea");
                        return;
                    }
                    GenerateRequest();
                }, () => _generateRequestCommandCanExecute));
            }
        }

        private RelayCommand _copyCommand;
        public RelayCommand CopyCommand
        {
            get
            {
                return _copyCommand ?? new RelayCommand(async () =>
                {
                    this.License.Log("frmActivateManually.btnCopy_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    try
                    {
                        if (string.IsNullOrEmpty(ActivationRequestContent))
                            return;
                        Clipboard.SetDataObject(ActivationRequestContent, false);
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.Copied, Type = NotificationType.Information }, areaName: "WindowArea");
                    }
                    catch (Exception exception)
                    {
                        this.License.Log("Error copying activation request with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                    }
                    this.License.Log("frmActivateManually.btnCopy_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                }, () => _copyCommandCanExceute);

            }
        }

        private RelayCommand _pasteCommand;
        public RelayCommand PasteCommand
        {
            get
            {
                return _pasteCommand != null ? _pasteCommand : _pasteCommand = new RelayCommand(() =>
                {
                    this.License.Log("frmActivateManually.btnPaste_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    try
                    {
                        ActivationContent = Clipboard.GetText();
                    }
                    catch (Exception exception)
                    {
                        this.License.Log("Error pasting activation code with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                    }
                    this.License.Log("frmActivateManually.btnPaste_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                }, () => _pasteEnabled);

            }
        }

        private RelayCommand _loadActivationCommand;
        public RelayCommand LoadActivationCommand
        {
            get
            {
                return _loadActivationCommand != null ? _loadActivationCommand : _loadActivationCommand = new RelayCommand(() =>
                {
                    this.License.Log("frmActivateManually.btnLoadActivationCode_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    try
                    {
                        Stream input = null;
                        this.openFileDialog.InitialDirectory = @"C:\";
                        this.openFileDialog.Filter = "xml files (*.xml)|*.xml";
                        this.openFileDialog.FilterIndex = 2;
                        this.openFileDialog.RestoreDirectory = true;
                        this.openFileDialog.Title = "Load Activation Code";
                        if ((bool)this.openFileDialog.ShowDialog())
                        {
                            input = this.openFileDialog.OpenFile();
                            Stream stream2 = input;
                            try
                            {
                                XmlTextReader reader = new XmlTextReader(input)
                                {
                                    WhitespaceHandling = WhitespaceHandling.None
                                };
                                XmlDocument document1 = new XmlDocument();
                                document1.Load(reader);
                                string innerXml = document1.InnerXml;
                                ActivationContent = innerXml;
                            }
                            catch (XmlException exception)
                            {
                                string text1;
                                if (exception != null)
                                {
                                    text1 = exception.ToString();
                                }
                                else
                                {
                                    XmlException local1 = exception;
                                    text1 = null;
                                }
                                MessageBox.Show("The Activation Code File could not be read. " + text1);
                            }
                            finally
                            {
                                if (stream2 != null)
                                {
                                    stream2.Dispose();
                                }
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        this.License.Log("Error loading activation code with the error message: " + exception2.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                    }
                    this.License.Log("frmActivateManually.btnLoadActivationCode_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                }, () => _loadActivationEnabled);

            }
        }

        private RelayCommand _openActivationCommand;
        public RelayCommand OpenActivationCommand
        {
            get
            {
                return _openActivationCommand ?? new RelayCommand(() =>
                {
                    WebServiceHelper.OpenManualRequestUrl();
                });
            }
        }

        private RelayCommand _activationCommand;
        public RelayCommand ActivationCommand
        {
            get
            {
                return _activationCommand != null ? _activationCommand : _activationCommand = new RelayCommand(async () =>
                {
                    this.License.Log("frmActivateManually.btnActivation_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    try
                    {
                        if (string.IsNullOrEmpty(ActivationContent))
                        {
                            await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.InvalidActivecode, Type = NotificationType.Warning }, areaName: "WindowArea");
                            return;
                        }
                        ManualActivation();

                    }
                    catch (Exception exception)
                    {
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = exception.Message, Type = NotificationType.Warning }, areaName: "WindowArea");

                        this.License.Log("Error activating license with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                    }
                    this.License.Log("frmActivateManually.btnActivation_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                }, () => _activationCommandEnabled);
            }
        }

        private RelayCommand _saveActivationCommand;
        public RelayCommand SaveActivationCommand
        {
            get
            {
                return _saveActivationCommand != null ? _saveActivationCommand : _saveActivationCommand = new RelayCommand(async () =>
                {
                    this.License.Log("frmActivateManually.btnSaveActivationRequest_Click(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
                    try
                    {
                        if (string.IsNullOrEmpty(ActivationRequestContent))
                        {
                            await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.InvalidRequest, Type = NotificationType.Warning }, areaName: "WindowArea");
                            return;
                        }

                        await SaveActivation();

                    }
                    catch (Exception exception)
                    {
                        this.License.Log("Error saving activation request with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                    }
                    this.License.Log("frmActivateManually.btnSaveActivationRequest_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);

                }, () => _saveActivationEnabled);
            }
        }

        private RelayCommand<Object> _passwordChangedCommand;
        public RelayCommand<Object> PasswordChangedCommand
        {
            get
            {
                return _passwordChangedCommand ?? (_passwordChangedCommand = new RelayCommand<Object>(obj =>
                {
                    if (obj == null) return;
                    _password = ((PasswordBox)obj).Password;
                }));
            }
        }

        private RelayCommand<object> _closeCommand;
        public RelayCommand<object> CloseCommand
        {
            get
            {
                return _closeCommand != null ? _closeCommand : _closeCommand = new RelayCommand<object>(obj =>
                {
                    this.IsSelected = false;
                    Clear();
                    var model = _licenseService.GetLicenseViewModel();
                    Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                });
            }
        }

        public ActiveManualViewModel(IServiceProvider serviceProvider, LicenseType licenseType, string name)
        {
            _name = name;
            _serviceProvider = serviceProvider;
            _licenseType = licenseType;
            InitialzeViewModel();

            SelectCommand = new RelayCommand(() =>
            {
                IsSelected = true;
                ReloadViewModel();
                Broadcaster.SendMessage(new ShowActiveManualLicenseViewMessage(new ShowActiveManualLicenseDetailData(this)));
            });
        }

        private void ReloadViewModel()
        {
            GenerateRequestCommandCanExecute = ActivationCommandEnabled = _sKLicenseManager.License.Status != LicenseStatus.LicenseReady;
            if (_sKLicenseManager.License.Status != LicenseStatus.LicenseReady)
            {
                _sKLicenseManager.LicenseType = _licenseType;
                License = _sKLicenseManager.License;
            }
            NetworkVisible = !isNetworkMode ? Visibility.Hidden : Visibility.Visible;
            NetworkPath = NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue;
            NetworkPathColor = Brushes.Green;
        }
        private void GenerateRequest()
        {
            this.License.Log("GenerateRequest; [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            try
            {
                int result = int.Parse(LicenseId);
                this.License.ResetSessionCode();
                GenerateRequestCommandCanExecute = false;
                ActivationRequestContent = this.License.GetActivationInstallationLicenseFileRequest(result, _password);
                GenerateRequestCommandCanExecute = true;
                CopyCommandCanExceute = true;
                PasteEnabled = SaveActivationEnabled = LoadActivationEnabled = true;
            }
            catch (Exception exception)
            {
                this.License.Log("Error generating request with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            }
            this.License.Log("frmActivateManually.btnGenerateRequest_Click(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
        }
        private string createSessionCode()
        {
            try
            {
                return this.License.CurrentSessionCode;
            }
            catch (Exception)
            {
                return "";
            }
        }
        private void deleteBackupFiles()
        {
            this.License.Log("frmActivateManually.deleteBackupFiles(); [Start]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
            try
            {
                if (File.Exists(this.pathFileOne))
                {
                    File.Delete(this.pathFileOne);
                }
                if (File.Exists(this.pathFileTwo))
                {
                    File.Delete(this.pathFileTwo);
                }
            }
            catch (Exception exception)
            {
                this.License.Log("Unable to delete an backup file with the error message: " + exception.Message, MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
            }
            this.License.Log("frmActivateManually.deleteBackupFiles(); [End]", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Info);
        }
        private async Task SaveActivation()
        {
            this.saveFileDialog.InitialDirectory = @"C:\";
            this.saveFileDialog.Filter = "xml files (*.xml)|*.xml";
            this.saveFileDialog.FilterIndex = 2;
            this.saveFileDialog.RestoreDirectory = true;
            this.saveFileDialog.Title = CommonStringsAdmin.UIStrings.SaveActivation;

            if ((bool)this.saveFileDialog.ShowDialog())
            {
                pathFileOne = this.saveFileDialog.FileName;
                pathFileTwo = this.saveFileDialog.InitialDirectory + "_" + this.saveFileDialog.SafeFileName;
                StreamWriter writer = new StreamWriter(this.saveFileDialog.OpenFile());
                int index = 0;
                while (true)
                {
                    if (index >= ActivationRequestContent.Split('\n').Length)
                    {
                        writer.Dispose();
                        writer.Close();
                        this.m_sessionCode = this.createSessionCode();
                        StreamWriter writer1 = new StreamWriter(this.pathFileOne);
                        writer1.WriteLine(this.m_sessionCode);
                        writer1.Dispose();
                        writer1.Close();
                        try
                        {
                            if (File.Exists(this.pathFileOne))
                            {
                                File.SetAttributes(this.pathFileOne, FileAttributes.Hidden);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        StreamWriter writer2 = new StreamWriter(this.pathFileTwo);
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 >= ActivationRequestContent.Split('\n').Length)
                            {
                                writer2.Dispose();
                                writer2.Close();
                                try
                                {
                                    if (File.Exists(this.pathFileTwo))
                                    {
                                        File.SetAttributes(this.pathFileTwo, FileAttributes.Hidden);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                break;
                            }
                            writer2.WriteLine(ActivationRequestContent.Split('\n')[num2].ToString());
                            num2++;
                        }
                        break;
                    }
                    writer.WriteLine(ActivationRequestContent.Split('\n')[index].ToString());
                    index++;
                }
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.Saved, Type = NotificationType.Information }, areaName: "WindowArea");
            }
        }
        private void InitialzeViewModel()
        {
            _sKLicenseManager = _serviceProvider.GetRequiredService<ISKLicenseManagerService>();
            _licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            _notification = _serviceProvider.GetRequiredService<INotificationManager>();
            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();
            GenerateRequestCommandCanExecute = ActivationCommandEnabled = PasteEnabled =
            SaveActivationEnabled = LoadActivationEnabled = CopyCommandCanExceute = true;
            //**********
            base.AddRule(() => LicenseId, () =>
            !string.IsNullOrEmpty(LicenseId) &&
            LicenseId.Length > 1 && int.TryParse(LicenseId, out int result) &&
            LicenseId.Length < 100, CommonStringsAdmin.UIStrings.LicenseIdValidation);

            base.AddRule(() => Password, () =>
            !string.IsNullOrEmpty(Password), CommonStringsAdmin.UIStrings.PasswordRequierd);
        }
        private async void ManualActivation()
        {
            try
            {

                LoadNetworkPathAsync();
                string licenseContent = string.Empty;
                ActivationCommandEnabled = false;
                activationResult = License.ProcessActivateInstallationLicenseFileResponse(ActivationContent, ref licenseContent);

                if (activationResult)
                {
                    if (this.License.SaveLicenseFile(licenseContent))
                    {
                        this.deleteBackupFiles();
                        this.License.ResetSessionCode();
                        this.License.UpdateStatus(true);
                        NetworkLicenseConfiguration.GetConfiguration.NetworkEnable = _licenseType == LicenseType.Network;
                        var model = _licenseService.GetLicenseViewModel();
                        await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = CommonStringsAdmin.UIStrings.ActivationLicenseSuccessful, Type = NotificationType.Success }, areaName: "WindowArea");
                        Clear();
                        Broadcaster.SendMessage(new ShowLicenseViewMessage(new ShowLicenseDetailData(model)));
                    }
                    else
                    {
                        string caption = string.Empty;
                        if (this.License.ProdConfig != null)
                        {
                            caption = this.License.ProdConfig.ProductName;
                        }
                        else
                        {
                            caption = $"[{CommonStringsAdmin.UIStrings.ProductName}]";
                            this.License.Log("ProductLicenseConfiguration not assigned to LicenseManager.", MethodBase.GetCurrentMethod(), LicenseLoggingsLevels.Error);
                        }
                        await _notification.ShowAsync(new NotificationContent { Title = caption, Message = CommonStringsAdmin.UIStrings.ActivationFailed, Type = NotificationType.Error }, areaName: "WindowArea");
                    }
                }
                else
                {
                    bool flag;
                    string text = !this.License.LastError.ToString().Contains("check Internet connectivity") ? this.License.licenseError(this.License.LastError.ExtendedErrorNumber, out flag) : CommonStringsAdmin.UIStrings.ActiveManulFailed;
                    await _notification.ShowAsync(new NotificationContent { Title = text, Message = CommonStringsAdmin.UIStrings.Activation, Type = NotificationType.Error }, areaName: "WindowArea");
                }
                ActivationCommandEnabled = true;

            }
            catch (Exception ex)
            {
                await _notification.ShowAsync(new NotificationContent { Title = CommonStringsAdmin.UIStrings.Activation, Message = ex.Message, Type = NotificationType.Error }, areaName: "WindowArea");

            }
        }
        private void LoadNetworkPathAsync()
        {
            if (this._licenseType == LicenseType.Flat)
                return;

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
        private void CreateNetwork()
        {
            NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue = NetworkPath;
            NetworkLicenseConfiguration.GetConfiguration.LicenseFilePath = NetworkLicenseConfiguration.GetConfiguration.PathRegistryValue;
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
        private void Clear()
        {
            ActivationContent = ActivationRequestContent = LicenseId = string.Empty;
        }

    }
}

