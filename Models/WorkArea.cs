using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamline.Module.Admin.Models
{
    public class WorkArea
    {
        public string? StationName { get; set; }
        public string? CommonName { get; set; }
        public string? UnitId { get; set; }
        private List<User> _users;
        public List<User> Users
        {
            get
            {
                if (_users == null)
                    _users = new List<User>();
                return _users;
            }
        }
    }
}
