using NbfcClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace NbfcClient.DesignData
{
    public class FanControllerViewModelCollection : ObservableCollection<FanControllerViewModel>
    {
        public FanControllerViewModelCollection()
            : base()
        {
        }
    }
}
