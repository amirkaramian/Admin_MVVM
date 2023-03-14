using MahApps.Metro.Controls;
using Streamline.Module.Admin.ViewModel;
using Streamline.Module.Admin.ViewModel.License;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Streamline.Module.Admin
{
    [SuppressMessage("NDepend", "ND1700:PotentiallyDeadTypes", Justification = "This class is reffered from XAML")]
    internal sealed class HamburgerItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;

            if (element != null && item != null && item is HamburgerMenuItem hmi)
            {
               if (hmi.Tag is UserMapsViewModel)
                {
                    return element.FindResource("userMapTemplate") as DataTemplate;
                }
                else if (hmi.Tag is LicenseItemsViewModel)
                {
                    return element.FindResource("licenseTemplate") as DataTemplate;
                }
            }

            return null;

        }
    }
}
