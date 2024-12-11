using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace PhoenixWPF.Helper
{
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Recursively searches the VisualTree to find a ctrl with the specified name.
        /// Usage example: ctrl propertyctrl = VisualTreeHelperExtensions.FindctrlByName(yourRootElement);
        /// </summary>
        /// <param name="parent">The parent DependencyObject to start the search from.</param>
        /// <param name="ctrlName">The name of the ctrl to find. Default is "Propertyctrl".</param>
        /// <returns>The ctrl if found, otherwise null.</returns>
        public static Control? FindControlByName(DependencyObject parent, string ctrlName)
        {
            if (parent == null)
                return null;

            // Check if the current object is a ctrl with the specified name
            if (parent is Control ctrl && ctrl.Name == ctrlName)
                return ctrl;

            // Recursively search the children of the parent
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                Control? result = FindControlByName(child, ctrlName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

