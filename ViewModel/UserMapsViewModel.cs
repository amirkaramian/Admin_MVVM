
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using SmartInspectFor3DInfotech;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Messages;
using Streamline.Common;
using Streamline.Common.Interfaces;
using Streamline.Common.Messaging;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Streamline.Module.Admin.Messages.ShowUserMapDetailData;
using CommonStringsAdmin;

namespace Streamline.Module.Admin.ViewModel
{
    internal sealed partial class UserMapsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<UserMapViewModel> _items = new ObservableCollection<UserMapViewModel>();

        [ObservableProperty]
        private PackIconControlBase _icon = new PackIconUnicons { Kind = PackIconUniconsKind.User };

        [ObservableProperty]
        private PackIconControlBase _iconSmall = new PackIconUnicons
        {
            Kind = PackIconUniconsKind.User,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Height = 32,
            Width = 32
        };
        private RelayCommand _addNewUserCommnad;
        public RelayCommand AddNewUserCommnad
        {
            get
            {
                return _addNewUserCommnad ?? new RelayCommand(() =>
                {
                    var service = _serviceProvider.GetRequiredService<IUserMapService>();
                    if (!_items.Any(x => x.IsNew))
                    {
                        foreach (var item in _items)
                            item.IsSelected = false;
                        var model = new UserMapViewModel(_serviceProvider, service)
                        {
                            StationCommonName = service.Station.CommonName,
                            StationName = service.Station.StationName,
                            StationUnitId = service.Station.UnitId,
                            Caption = UIStrings.NewUser,
                            PasswordHash = "",
                            IsNew = true,
                        };                        
                        _items.Add(model);

                        Broadcaster.SendMessage(new ShowNewUserMapViewMessage(new ShowNewUserMapData(model)));
                    }
                });
            }
        }


        public UserMapsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var service = _serviceProvider.GetRequiredService<IUserMapService>();
            service.LoadWorkAreas();
            foreach (var item in service.MapViewModel)
                _items.Add(item);
            StrongReferenceMessenger.Default.Register<ShowNewUserMapViewMessage>(this, ShowNewUserMapView);
            StrongReferenceMessenger.Default.Register<ShowAddUserMapViewMessage>(this, ShowAddNewUserMapView);
            StrongReferenceMessenger.Default.Register<ShowRemoveUserMapViewMessage>(this, ShowRemoveUserMapView);
        }
        public List<UserMapViewModel> RefreshData()
        {
            var service = _serviceProvider.GetRequiredService<IUserMapService>();
            service.LoadWorkAreas();
            return service.MapViewModel;
        }
        private void ShowAddNewUserMapView(object recipient, ShowAddUserMapViewMessage message)
        {
            Items = new ObservableCollection<UserMapViewModel>(RefreshData());
            var item = Items.FirstOrDefault(x => x.EmployeeID == ((UserMapViewModel)message.Value.Data).EmployeeID);
            if (item != null)
            {
                item.IsSelected = true;
                Broadcaster.SendMessage(new ShowUserMapDetailViewMessage(new ShowUserMapDetailData(item)));
            }
        }
        private void ShowNewUserMapView(object recipient, ShowNewUserMapViewMessage message)
        {
            Broadcaster.SendMessage(new ShowUserMapDetailViewMessage(new ShowUserMapDetailData(message.Value.Data)));

        }
        private void ShowRemoveUserMapView(object recipient, ShowRemoveUserMapViewMessage message)
        {
            Items = new ObservableCollection<UserMapViewModel>(RefreshData());
            var item = Items.First();
            item.IsSelected = true;
            Broadcaster.SendMessage(new ShowUserMapDetailViewMessage(new ShowUserMapDetailData(item)));
        }
    }
}
