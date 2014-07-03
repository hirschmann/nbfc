using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ConfigEditor.Windows
{
    public abstract class NonModalDialogWindow : Window
    {
        public bool ApplyChanges { get; set; }
    }
}
