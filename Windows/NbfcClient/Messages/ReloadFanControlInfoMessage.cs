using GalaSoft.MvvmLight.Messaging;

namespace NbfcClient.Messages
{
    public class ReloadFanControlInfoMessage : MessageBase
    {
        public ReloadFanControlInfoMessage()
        {
        }

        public ReloadFanControlInfoMessage(bool ignoreCache)
        {
            IgnoreCache = ignoreCache;
        }

        public bool IgnoreCache { get; set; }
    }
}
