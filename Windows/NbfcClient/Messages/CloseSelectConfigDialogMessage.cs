using GalaSoft.MvvmLight.Messaging;

namespace NbfcClient.Messages
{
    public class CloseSelectConfigDialogMessage : MessageBase
    {
        public CloseSelectConfigDialogMessage()
        {
        }

        public CloseSelectConfigDialogMessage(bool dialogResult, string selectedConfig)
        {
            DialogResult = dialogResult;
            SelectedConfig = selectedConfig;
        }

        public bool DialogResult { get; set; }
        public string SelectedConfig { get; set; }
    }
}
