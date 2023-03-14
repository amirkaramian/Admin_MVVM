using Streamline.Module.Admin.Models;
using Streamline.Module.Admin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Interfaces
{
    internal interface IUserMapService
    {
        List<UserMapViewModel> MapViewModel { get; }
        WorkArea Station { get;  }
        List<WorkArea> GetWorkAreas();

        void LoadWorkAreas();        
        void StoreWorkAreas();
        void AddUserToArea(User user);
        bool RemoveUserFromArea(User user);

    }
}
