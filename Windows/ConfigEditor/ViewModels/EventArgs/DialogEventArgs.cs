using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigEditor.ViewModels
{
    public class DialogEventArgs<T> : EventArgs
    {
        public bool Update { get; set; }
        public T ViewModel { get; private set; }

        public DialogEventArgs(T viewModel)
        {
            this.ViewModel = viewModel;
        }
    }
}
