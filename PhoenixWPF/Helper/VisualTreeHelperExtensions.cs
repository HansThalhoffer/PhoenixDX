using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhoenixWPF.Helper {
    /// <summary>
    /// Erweiterungsmethoden für den VisualTreeHelper zur Erleichterung der UI-Elementsuche.
    /// </summary>
    public static class VisualTreeHelperExtensions {
        /// <summary>
        /// Sucht den übergeordneten Container eines bestimmten Typs im VisualTree.
        /// </summary>
        /// <typeparam name="T">Der Typ des übergeordneten Containers.</typeparam>
        /// <param name="child">Das untergeordnete DependencyObject, von dem aus die Suche beginnt.</param>
        /// <returns>Das gefundene übergeordnete Element des angegebenen Typs oder null, falls keines gefunden wurde.</returns>
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null && !(parentObject is T)) {
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return parentObject as T;
        }

        /// <summary>
        /// Durchsucht rekursiv den VisualTree, um ein Control mit dem angegebenen Namen zu finden.
        /// </summary>
        /// <param name="parent">Das übergeordnete DependencyObject, von dem aus die Suche beginnt.</param>
        /// <param name="ctrlName">Der Name des gesuchten Controls. Standard ist "Propertyctrl".</param>
        /// <returns>Das gefundene Control, falls vorhanden, andernfalls null.</returns>
        public static Control? FindControlByName(DependencyObject parent, string ctrlName) {
            if (parent == null)
                return null;

            // Überprüft, ob das aktuelle Objekt ein Control mit dem angegebenen Namen ist
            if (parent is Control ctrl && ctrl.Name == ctrlName)
                return ctrl;

            // Durchsucht rekursiv die untergeordneten Elemente des übergeordneten Elements
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                Control? result = FindControlByName(child, ctrlName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}