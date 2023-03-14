using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Streamline.Module.Admin.Interfaces;
using Streamline.Module.Admin.Models;
using Streamline.Module.Admin.ViewModel;
using Streamline.Common;
using Streamline.Common.Interfaces;
using Streamline.Common.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Path = System.IO.Path;

namespace Streamline.Module.Admin.Services
{
    internal sealed class UserMapService : ServiceBase, IUserMapService
    {
        private Node _workAreas;
        private XmlSerializer _serializer;
        private List<UserMapViewModel> _userMap;
        private IServiceProvider _serviceProvider;
        public List<UserMapViewModel> MapViewModel => _userMap;
        public WorkArea Station { get; private set; }
        private string _filePath;
        public UserMapService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serializer = new XmlSerializer(typeof(Node));
            _serviceProvider = serviceProvider;
            var propertBag = _serviceProvider.GetRequiredService<IPropertyBag>();
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "3D Infotech\\Streamline2\\admin"); //propertBag.GetValue<string>("UacData");
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);            
            if (propertBag.GetValue<string>("UacData", out var filename) &&  string.IsNullOrEmpty(filename))
                filename = "Uac.xml";
            _filePath += $"\\{filename}";
            ValidateXmlDataFile();
        }
        private void ValidateXmlDataFile()
        {
            if (!File.Exists(_filePath))
            {
                var template = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                                <Node Name=""Station"" IsPlain=""true"">
                                  <Node Name=""CommonName"" DataType=""String"" Value="""" />
                                  <Node Name=""UnitID"" DataType=""String"" Value="""" />
                                  <Node Name=""UAC"">
                                    
                                  </Node>
                                </Node>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(template);
                doc.Save(_filePath);
            }
        }

        public void AddUserToArea(User user)
        {
            if (_userMap.Any(x => x.EmployeeID == user.EmployeeId && x.UniqueId != user.UniqueId))
                throw new Exception("multiple employeeId value");
            if (_userMap.Any(x => x.Name.ToLower() == user.Name.ToLower() && x.UniqueId != user.UniqueId))
                throw new Exception("name already existed");

            foreach (var item in _workAreas.Node1)
            {
                if (item.Name.ToLower() == "commonname")
                    item.Value = user.StationCommonName;
                if (item.Name.ToLower() == "unitid")
                    item.Value = user.StationUnitId;
            }
            var workMain = _workAreas.Node1.First(x => x.Name.ToLower() == "uac");
            NodeNodeNode work = null;
            if (workMain == null)
            {
                _workAreas.Node1.Add(new NodeNode()
                {
                    Name = "UAC"
                });

            }

            work = workMain.Node.FirstOrDefault(x => x.Name == user.Name);



            if (work == null)
            {

                work = new NodeNodeNode
                {
                    Name = user.Name,
                    Node = new List<NodeNodeNodeNode>()
                {
                    new NodeNodeNodeNode{ Name = "CommonName",DataType = "String",Value = user.CommonName,},
                    new NodeNodeNodeNode{ Name = "EmployeeID",DataType = "String",Value = user.EmployeeId},
                    new NodeNodeNodeNode{ Name = "StationRole(s)",DataType = "String",Value = string.Join(",", user.StationRoles)},
                    new NodeNodeNodeNode{ Name = "LoginName",DataType = "String",Value = user.LoginName},
                    new NodeNodeNodeNode{ Name = "PasswordHash",DataType = "String",Value = user.PasswordHash}
                }
                };

                _workAreas.Node1[2].Node.Add(work);
                return;
            }

            foreach (var item in work.Node)
                switch (item.Name.ToLower())
                {
                    case "commonname":
                        item.Value = user.CommonName;
                        break;
                    case "employeeid":
                        item.Value = user.EmployeeId;
                        break;
                    case "stationrole(s)":
                        item.Value = string.Join(",", user.StationRoles);
                        break;
                    case "loginname":
                        item.Value = user.LoginName;
                        break;
                    case "passwordhash":
                        item.Value = user.PasswordHash;
                        break;
                    default:
                        break;
                }

        }

        public List<WorkArea> GetWorkAreas()
        {
            return _workAreas.Node1.Select(x => new WorkArea()
            {
                CommonName = x.Name,
                StationName = string.Join(",", x.Value),
                UnitId = x.Value,
            }).ToList();
        }

        public void LoadWorkAreas()
        {
            string xml = File.ReadAllText(_filePath);
            var stReader = new StringReader(xml);
            _workAreas = (Node)_serializer.Deserialize(stReader);
            _userMap = new List<UserMapViewModel>();
            if (_workAreas == null)
                return;
            Station = new WorkArea
            {
                StationName = _workAreas.Name,
                CommonName = _workAreas.Node1.FirstOrDefault(x => x.Name == "CommonName")?.Value,
                UnitId = _workAreas.Node1.FirstOrDefault(x => x.Name == "UnitID")?.Value
            };
            var workMain = _workAreas.Node1.FirstOrDefault(x => x.Name.ToLower() == "uac");
            if (workMain == null)
                return;
            var items = workMain.Node;
            foreach (var item in items)
            {
                var user = new UserMapViewModel(_serviceProvider, this) { Name = item.Name, };
                user.UniqueId = Guid.NewGuid();
                user.StationName = Station?.StationName ?? string.Empty;
                user.StationCommonName = Station?.CommonName ?? string.Empty;
                user.StationUnitId = Station?.UnitId ?? string.Empty;
                foreach (var node in item.Node)
                {
                    switch (node.Name)
                    {
                        case "CommonName":
                            user.CommonName = node.Value;
                            user.Caption = node.Value;
                            break;
                        case "EmployeeID":
                            user.EmployeeID = node.Value;
                            break;
                        case "StationRole(s)":
                            user.StationRole = node.Value;
                            break;
                        case "LoginName":
                            user.LoginName = node.Value;
                            break;
                        case "PasswordHash":
                            user.PasswordHash = node.Value;
                            break;
                        default:
                            break;
                    }
                }
                _userMap.Add(user);
            }
        }

        public bool RemoveUserFromArea(User user)
        {
            var item = _workAreas.Node1[2].Node.FirstOrDefault(x => x.Name == user.Name);
            if (item != null)
                return _workAreas.Node1[2].Node.Remove(item);
            return false;
        }

        public void StoreWorkAreas()
        {
            var xml = new StringWriter();
            var writer = XmlWriter.Create(xml);
            _serializer.Serialize(writer, _workAreas);
            var doc = new XmlDocument();
            doc.LoadXml(xml.ToString());
            doc.Save(_filePath);
        }
    }
}
