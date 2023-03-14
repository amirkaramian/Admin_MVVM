
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Streamline.Common;
using Streamline.Common.Interfaces;
using Streamline.Common.MVVM;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Services;
using Streamline.Module.Admin.ViewModel;
using Streamline.Module.Admin.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Notifications.Wpf.Core;
using Streamline.Module.Admin.ViewModel.License;
using TDI.Licensing.Interfaces;
using TDI.Licensing;
using Streamline.Module.Admin.Views.License;
using CommunityToolkit.Mvvm.Messaging;
using CommonStringsAdmin;

namespace Streamline.Module.Admin
{
    [Module]
    public class Module : IModule
    {
        private bool _isCurrentModule = false;
        private AdminViewModel? _viewModel;
        private IServiceProvider? _serviceProvider;
        private bool disposed;
        private bool _initialized;

        public ViewModelBase ViewModel { get; private set; }

        public IModuleView MainView { get; private set; }

        public string DisplayName => UIStrings.AdminModuleName;

        public Control Icon { get; private set; } = new PackIconUnicons { Kind = PackIconUniconsKind.Setting, Width = 32, Height = 32 };

        public int SortOrder => 1;

        public bool Initialized => _initialized;

        public bool IsCurrentModule => _isCurrentModule;

        public IHostBuilder ConfigureModule(IHostBuilder builder)
        {
            RegisterViewModel(builder);
            return builder;
        }



        public async Task Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Debug.WriteLine("Initializing Admin Module");
            ViewModel = _viewModel = _serviceProvider.GetRequiredService<AdminViewModel>();
            MainView = _serviceProvider.GetRequiredService<AdminView>();
            MainView.DataContext = ViewModel;
            _initialized = true;

            await Task.CompletedTask;
        }

        public Task Load()
        {
            _isCurrentModule = true;            
            return Task.CompletedTask;
        }

        public Task UnLoad()
        {
            _isCurrentModule = false;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                StrongReferenceMessenger.Default.UnregisterAll(this);
              

                ViewModel.Dispose();
            }
            disposed = true;
        }
        private void RegisterViewModel(IHostBuilder serviceDescriptors)
        {
            serviceDescriptors.ConfigureServices((_, services) =>
            {

                services.AddSingleton<IUserMapService, UserMapService>();
                services.AddSingleton<ILicenseService, LicenseService>();
                services.AddSingleton<ISKLicenseManagerService, SKLicenseManagerService>();
                services.AddScoped<INotificationManager>(x => new NotificationManager(Notifications.Wpf.Core.Controls.NotificationPosition.BottomLeft));

                services.AddSingleton<AdminViewModel>();
                services.AddScoped<UserMapsViewModel>();
                services.AddScoped<LicenseItemsViewModel>();

                services.AddSingleton<AdminView>();
            });
        }
    }
}
