using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Streamline.Common;
using Streamline.Common.Interfaces;
using Streamline.Common.Messaging;
using Streamline.Common.MVVM;
using Streamline.Common.Options;
using Streamline.Module.Admin.Messages;
using Streamline.Module.Admin.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Streamline.Module.Admin.Messages.ShowUserMapDetailViewMessage;
using Streamline.Module.Admin.ViewModel.License;

namespace Streamline.Module.Admin.ViewModel
{
    internal sealed partial class AdminViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StreamlineOptions _options;
        private readonly IPropertyBag _propertyBag;
       
        [ObservableProperty]
        private ViewModelBase? _selectedItem;

        [ObservableProperty]
        private UserMapsViewModel _userMapsViewModel;

        [ObservableProperty]
        private LicenseItemsViewModel _licenseItemsViewModel;

   
        public AdminViewModel(IServiceProvider serviceProvider, IOptions<StreamlineOptions> options)
        {

            _options = options.Value;

            // Create a dependency Scope here.
            IServiceScope serviceScope = serviceProvider.CreateScope();
            _serviceProvider = serviceScope.ServiceProvider;

            _propertyBag = _serviceProvider.GetRequiredService<IPropertyBag>();

            if (!string.IsNullOrEmpty(_options.UserDataFolder))
            {
                _propertyBag.AddValue(Constants.UserDataFolderKey, _options.UserDataFolder);
            }

            if (!string.IsNullOrEmpty(_options.AppDataFolder))
            {
                _propertyBag.AddValue(Constants.AppDataFolderKey, _options.AppDataFolder);
            }

            
            _userMapsViewModel = _serviceProvider.GetRequiredService<UserMapsViewModel>();
            _licenseItemsViewModel = _serviceProvider.GetRequiredService<LicenseItemsViewModel>();
            
            
            StrongReferenceMessenger.Default.Register<ShowUserMapDetailViewMessage>(this, ShowUserMapView);
            StrongReferenceMessenger.Default.Register<ShowLicenseViewMessage>(this, ShowLicenseView);
         

        }



        
        private void ShowUserMapView(object recipient, ShowUserMapDetailViewMessage message)
        {
            var newItem = _userMapsViewModel.Items.FirstOrDefault(x => x.IsNew);
            if (newItem != null)
                _userMapsViewModel.Items.Remove(newItem);
            SelectedItem = message.Value.Data;                       
        }


        private void ShowLicenseView(object reipient, ShowLicenseViewMessage message)
        {
            SelectedItem = message.Value.Data;
        }
    }
}
