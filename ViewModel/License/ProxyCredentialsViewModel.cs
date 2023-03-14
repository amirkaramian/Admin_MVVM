using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Streamline.Module.Admin.Views.License;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Streamline.Module.Admin.ViewModel.License
{
    internal sealed partial class ProxyCredentialsViewModel : ViewModelBase
    {
        private string m_Username;
        private string m_Password;
        private string m_ProxyAddress;
        private Window _window;
        public NetworkCredential Credentials => new NetworkCredential(this.m_Username, this.m_Password);

        public string ProxyAddress
        {
            get => this.m_ProxyAddress;
            set => this.m_ProxyAddress = value;
        }

        [ObservableProperty]
        private string _proxyServer;
        [ObservableProperty]
        private string _userName;
        [ObservableProperty]
        private string _password;

        private string proxyAddress;

        private RelayCommand _okCommand;
        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? new RelayCommand(() =>
                 {
                     this.m_Username = this.UserName;
                     this.m_Password = this.Password;
                     this.ProxyAddress = this.ProxyServer;
                     this._window.Close();
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
                    _window.Close();
                });
            }

        }
        public ProxyCredentialsViewModel(Window win,string proxyAddress)
        {
            this.proxyAddress = proxyAddress;
            _window = win;
        }
    }
}
