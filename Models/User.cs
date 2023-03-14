using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Models
{
    public class User
    {
        public string? StationCommonName { get; set; }
        public string? StationUnitId { get; set; }
        public string? Name { get; set; }
        public string? CommonName { get; set; }
        public string? EmployeeId { get; set; }
        public List<string>? StationRoles { get; set; }
        public string? PasswordHash { get; set; }
        public string? LoginName { get; set; }
        public bool IsNew { get; set; }
        public Guid UniqueId { get; set; }
    }
}
