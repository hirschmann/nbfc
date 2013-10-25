using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConfigEditor.Windows
{
    /// <summary>
    /// Interaction logic for RegisterWriteConfigWindow.xaml
    /// </summary>
    public partial class RegisterWriteConfigWindow : NonModalDialogWindow
    {
        #region Constructor

        public RegisterWriteConfigWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.ApplyChanges = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.ApplyChanges = false;
            this.Close();
        }

        #endregion
    }
}
