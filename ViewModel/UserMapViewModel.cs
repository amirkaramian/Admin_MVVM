
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using SmartInspectFor3DInfotech;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Messages;
using Streamline.Module.Admin.Models;
using Streamline.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommonStringsAdmin;

namespace Streamline.Module.Admin.ViewModel
{
    internal sealed partial class UserMapViewModel : ViewModelMaster
    {
        private static readonly HyperArea Logger = HyperLogger.MakeHyperLog(nameof(UserMapViewModel)).High;
        private static readonly HyperArea Detail = HyperLogger.MakeHyperLog(nameof(UserMapViewModel)).Detail;

        private IUserMapService _mapService;
        private INotificationManager _notification;
        public Guid UniqueId { get; set; }
        [ObservableProperty]
        private string _stationName;
        [ObservableProperty]
        private string _stationCommonName;
        [ObservableProperty]
        private string _stationUnitId;
        [ObservableProperty]
        private bool _isNew;
        [ObservableProperty]
        private string _caption;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _commonName;
        [ObservableProperty]
        private string _employeeID;
        [ObservableProperty]
        private string _stationRole;
        [ObservableProperty]
        private string _loginName;
        [ObservableProperty]
        private string _passwordHash;
        [ObservableProperty]
        private bool _isSelected;
        public RelayCommand SelectCommand { get; private set; }

        private RelayCommand _saveCommnad;
        public RelayCommand SaveCommnad
        {
            get
            {
                return _saveCommnad ?? new RelayCommand(async () =>
                {
                    try
                    {


                        if (base.HasErrors)
                        {
                            await _notification.ShowAsync(new NotificationContent { Title = UIStrings.UserAccess, Message = Error, Type = NotificationType.Warning }, areaName: "WindowArea");

                            return;
                        }
                        var selectedUser = new User()
                        {
                            StationCommonName = StationCommonName,
                            StationUnitId = StationUnitId,
                            CommonName = CommonName,
                            EmployeeId = EmployeeID,
                            LoginName = LoginName,
                            Name = Name,
                            PasswordHash = _passwordHash,
                            StationRoles = new List<string> { _stationRole },
                            IsNew = this.IsNew,
                            UniqueId = this.UniqueId,
                        };

                        if (MessageBox.Show(UIStrings.StoreuserQuesstion, UIStrings.UserAccess, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _mapService.AddUserToArea(selectedUser);
                            _mapService.StoreWorkAreas();
                            Broadcaster.SendMessage(new ShowAddUserMapViewMessage(new ShowAddUserMapData(this)));
                            this.IsNew = false;

                            await _notification.ShowAsync(new NotificationContent { Title = UIStrings.UserAccess, Message = UIStrings.UserStored, Type = NotificationType.Success }, areaName: "WindowArea");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _notification.ShowAsync(new NotificationContent { Title = UIStrings.UserAccess, Message = ex.Message, Type = NotificationType.Error }, areaName: "WindowArea");
                    }
                });
            }
        }

        private RelayCommand _removeCommnad;
        public RelayCommand RemoveCommnad
        {
            get
            {
                return _removeCommnad ?? new RelayCommand(async () =>
                {
                    if (!this.IsSelected)
                    {
                        return;
                    }
                    var selectedUser = new User()
                    {
                        CommonName = CommonName,
                        EmployeeId = EmployeeID,
                        LoginName = LoginName,
                        Name = Name,
                        PasswordHash = _passwordHash,
                        StationRoles = new List<string> { _stationRole }
                    };
                    if (MessageBox.Show(UIStrings.RemoveUserQuestion, UIStrings.UserAccess, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _mapService.RemoveUserFromArea(selectedUser);
                        _mapService.StoreWorkAreas();
                        Broadcaster.SendMessage(new ShowRemoveUserMapViewMessage(new ShowRemoveUserMapData(this)));

                        this.IsNew = false;


                        await _notification.ShowAsync(new NotificationContent { Title = UIStrings.UserAccess, Message = UIStrings.UserRemoved, Type = NotificationType.Success }, areaName: "WindowArea");
                    }
                });
            }
        }

        private RelayCommand _cancelCommnad;
        public RelayCommand CancelCommnad
        {
            get
            {
                return _cancelCommnad ?? new RelayCommand(() =>
                {
                    Broadcaster.SendMessage(new ShowRemoveUserMapViewMessage(new ShowRemoveUserMapData(this)));
                });
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
                    _passwordHash = ((PasswordBox)obj).Password;
                }));
            }
        }
        public UserMapViewModel(IServiceProvider serviceProvider, IUserMapService mapService)
        {
            _mapService = mapService;
            SetValidators();
            _notification = serviceProvider.GetRequiredService<INotificationManager>();
            SelectCommand = new RelayCommand(() =>
            {
                IsSelected = true;
                Broadcaster.SendMessage(new ShowUserMapDetailViewMessage(new ShowUserMapDetailData(this)));
            });

        }

        private void SetValidators()
        {
            base.AddRule(() => StationCommonName, () =>
            !string.IsNullOrEmpty(StationCommonName) &&
            StationCommonName.Length > 1 &&
            StationCommonName.Length < 100, UIStrings.ValidateCommonName);

            base.AddRule(() => StationUnitId, () =>
            !string.IsNullOrEmpty(StationUnitId) &&
            StationUnitId.Length > 1 &&
            StationUnitId.Length < 100, UIStrings.ValidateUnitId);

            base.AddRule(() => EmployeeID, () =>
            !string.IsNullOrEmpty(EmployeeID) &&
            EmployeeID.Length > 2 &&
            EmployeeID.Length < 100, UIStrings.ValidateEmployeeId);

            base.AddRule(() => this.Name, () =>
            !string.IsNullOrEmpty(Name) &&
            Name.Length > 1 &&
            Name.Length < 100, UIStrings.ValidateName);
            
            base.AddRule(() => this.CommonName, () =>
            !string.IsNullOrEmpty(CommonName) &&
            StationCommonName.Length > 1 &&
            StationCommonName.Length < 100, UIStrings.ValidateCommonName);
            
            base.AddRule(() => this.StationRole, () =>
            !string.IsNullOrEmpty(StationRole) &&
            StationRole.Length > 1 &&
            StationRole.Length < 100, UIStrings.ValidateStationRole);

            base.AddRule(() => this.PasswordHash, () =>
            !string.IsNullOrEmpty(PasswordHash) &&
            PasswordHash.Length > 7 &&
            PasswordHash.Length < 100, UIStrings.InvalidPassword);
        }
    }
}
