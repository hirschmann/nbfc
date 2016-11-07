using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using System.Windows;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for SelectConfigWindow.xaml
    /// </summary>
    public partial class SelectConfigWindow : Window
    {
        public SelectConfigWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<CloseSelectConfigDialogMessage>(this, Close);
        }

        private void Close(CloseSelectConfigDialogMessage msg)
        {
            this.Close();
        }
    }
}
