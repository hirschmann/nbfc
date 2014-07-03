using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ConfigEditor
{
    public static class MouseDeviceExtensionMethods
    {
        public static bool IsMouseOverElement(this MouseDevice device, Type elementType)
        {
            var element = device.DirectlyOver as FrameworkElement;

            return element != null 
                && element.Parent != null 
                && element.Parent.GetType().IsAssignableFrom(elementType);
        }
    }
}
