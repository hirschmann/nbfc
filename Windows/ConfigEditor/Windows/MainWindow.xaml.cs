using ConfigEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ConfigEditor.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        #region Constants

        private const string SelectConfigArgumentPrefix = "-s:";

        private readonly Style MessageBoxErrorStyle;
        private readonly Style MessageBoxInfoStyle;

        #endregion

        #region Private Fields

        private Dictionary<ViewModelBase, Window> currentlyEditedViewModels;

        #endregion

        #region Dependency Properties

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
        }

        internal static readonly DependencyPropertyKey IsEditingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            this.MessageBoxErrorStyle = new Style(typeof(Xceed.Wpf.Toolkit.MessageBox));
            this.MessageBoxErrorStyle.Setters.Add(new Setter
            {
                Property = Xceed.Wpf.Toolkit.MessageBox.ImageSourceProperty,
                Value = SystemIcons.Error.ToImageSource()
            });

            this.MessageBoxInfoStyle = new Style(typeof(Xceed.Wpf.Toolkit.MessageBox));
            this.MessageBoxInfoStyle.Setters.Add(new Setter
            {
                Property = Xceed.Wpf.Toolkit.MessageBox.ImageSourceProperty,
                Value = SystemIcons.Information.ToImageSource()
            });

            this.currentlyEditedViewModels = new Dictionary<ViewModelBase, Window>();

            var vm = new MainViewModel();
            vm.RequestingConfigName += vm_RequestingConfigName;
            vm.RequestingConfigPath += vm_RequestingConfigPath;
            vm.SaveConfigCommandExecuted += vm_SaveConfigCommandExecuted;
            vm.ImportConfigError += vm_ImportConfigError;
            vm.PropertyChanged += vm_PropertyChanged;

            this.DataContext = vm;

            string arg = Environment.GetCommandLineArgs().FirstOrDefault(
                x => x.StartsWith(SelectConfigArgumentPrefix, StringComparison.OrdinalIgnoreCase));

            if (arg != null)
            {
                vm.SelectConfigCommand.Execute(arg.Substring(SelectConfigArgumentPrefix.Length));
            }
        }

        #endregion

        #region Private Methods

        private void AddViewModel<T>(Collection<T> collection, T viewModel, NonModalDialogWindow window) where T : ViewModelBase
        {
            window.Closed += (sender, e) =>
            {
                if (window.ApplyChanges)
                {
                    collection.Add(viewModel);
                }

                this.currentlyEditedViewModels.Remove(viewModel);
                this.SetValue(MainWindow.IsEditingPropertyKey, this.currentlyEditedViewModels.Count > 0);
            };

            this.currentlyEditedViewModels.Add(viewModel, window);
            this.SetValue(MainWindow.IsEditingPropertyKey, this.currentlyEditedViewModels.Count > 0);
            window.Show();
        }

        private void EditViewModel<T>(Collection<T> collection, T viewModel, NonModalDialogWindow window) where T : ViewModelBase
        {
            if (this.currentlyEditedViewModels.ContainsKey(viewModel))
            {
                this.currentlyEditedViewModels[viewModel].Activate();
            }
            else
            {
                this.currentlyEditedViewModels.Add(viewModel, window);
                this.SetValue(MainWindow.IsEditingPropertyKey, this.currentlyEditedViewModels.Count > 0);

                window.Closed += (sender, e) =>
                {
                    if (window.ApplyChanges)
                    {
                        collection.Replace(viewModel, (T)window.DataContext);
                    }

                    this.currentlyEditedViewModels.Remove(viewModel);
                    this.SetValue(MainWindow.IsEditingPropertyKey, this.currentlyEditedViewModels.Count > 0);
                };

                window.Show();
            }
        }

        private void RemoveViewModel<T>(Collection<T> collection, T viewModel) where T : ViewModelBase
        {
            if (this.currentlyEditedViewModels.ContainsKey(viewModel))
            {
                this.currentlyEditedViewModels[viewModel].Close();
                this.currentlyEditedViewModels.Remove(viewModel);
                this.SetValue(MainWindow.IsEditingPropertyKey, this.currentlyEditedViewModels.Count > 0);
            }

            if (this.DataContext is MainViewModel)
            {
                collection.Remove(viewModel);
            }
        }

        private void BeginEditFanConfig()
        {
            var dc = this.DataContext as MainViewModel;
            var vm = this.fanConfigsGrid.SelectedValue as FanConfigViewModel;

            if (vm != null && dc != null)
            {
                var clonedViewModel = (FanConfigViewModel)vm.Clone();
                clonedViewModel.Parent = dc;

                var wnd = new FanConfigWindow
                {
                    DataContext = clonedViewModel,
                    Owner = this
                };

                EditViewModel(dc.FanConfigs, vm, wnd);
            }
        }

        private void BeginEditRegisterWriteConfig()
        {
            var dc = this.DataContext as MainViewModel;
            var vm = this.registerWriteConfigsGrid.SelectedValue as RegisterWriteConfigViewModel;

            if (vm != null && dc != null)
            {
                var clonedViewModel = vm.Clone() as RegisterWriteConfigViewModel;

                var wnd = new RegisterWriteConfigWindow
                {
                    DataContext = clonedViewModel,
                    Owner = this
                };

                EditViewModel(dc.RegisterWriteConfigs, vm, wnd);
            }
        }

        #endregion

        #region Event Handlers

        #region ViewModel

        void vm_RequestingConfigName(object sender, DialogEventArgs<RequestConfigNameViewModel> e)
        {
            var dialog = new RequestConfigNameWindow
            {
                DataContext = e.ViewModel,
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                e.Update = true;
            }
        }

        void vm_RequestingConfigPath(object sender, DialogEventArgs<RequestConfigPathViewModel> e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "NBFC config file V2(*.xml)|*.xml|NBFC config file V1(*.config)|*.config",
                DefaultExt = "xml",
                Multiselect = false,
                Title = "Please select a config file"
            };

            if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                e.Update = true;
                e.ViewModel.ConfigFilePath = dialog.FileName;
            }
        }

        void vm_SaveConfigCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            if (e.Success)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    this,
                    "Config saved successfully",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxInfoStyle);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    this,
                    "Config could not be saved.\n\n" + e.Exception.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxErrorStyle);
            }
        }

        void vm_ImportConfigError(object sender, System.EventArgs e)
        {
            Xceed.Wpf.Toolkit.MessageBox.Show(
                            "The selected file could not be imported. Maybe it is not valid or you don't have read permissions",
                            "Import failed",
                            MessageBoxButton.OK,
                            MessageBoxErrorStyle);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedConfigName")
            {
                foreach (Window wnd in this.currentlyEditedViewModels.Values.ToArray())
                {
                    wnd.Close();
                }
            }
        }

        #endregion

        #region Notebook Model Buttons

        private void insertNotebookModel_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;

            if (vm != null)
            {
                this.notebookModel.Text = vm.ActualNotebookModel;
            }
        }

        #endregion

        #region Fan Configs

        private void addFanConfig_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as MainViewModel;

            if (dc != null)
            {
                var vm = new FanConfigViewModel
                {
                    Parent = dc
                };

                var wnd = new FanConfigWindow
                {
                    DataContext = vm,
                    Owner = this
                };

                AddViewModel(dc.FanConfigs, vm, wnd);
            }
        }

        private void editFanConfig_Click(object sender, RoutedEventArgs e)
        {
            BeginEditFanConfig();
        }

        private void removeFanConfig_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as MainViewModel;
            var vm = this.fanConfigsGrid.SelectedValue as FanConfigViewModel;

            if (vm != null && dc != null)
            {
                RemoveViewModel(dc.FanConfigs, vm);
            }
        }

        private void fanConfigsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MouseDevice.IsMouseOverElement(typeof(DataGridCell)))
            {
                BeginEditFanConfig();
            }
        }

        #endregion

        #region Register Write Configs

        private void addRegisterWriteConfig_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as MainViewModel;

            if (dc != null)
            {
                var vm = new RegisterWriteConfigViewModel();
                var wnd = new RegisterWriteConfigWindow
                {
                    DataContext = vm,
                    Owner = this
                };

                AddViewModel(dc.RegisterWriteConfigs, vm, wnd);
            }
        }

        private void editRegisterWriteConfig_Click(object sender, RoutedEventArgs e)
        {
            BeginEditRegisterWriteConfig();
        }

        private void removeRegisterWriteConfig_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as MainViewModel;
            var vm = this.registerWriteConfigsGrid.SelectedValue as RegisterWriteConfigViewModel;

            if (vm != null && dc != null)
            {
                RemoveViewModel(dc.RegisterWriteConfigs, vm);
            }
        }

        private void registerWriteConfigsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MouseDevice.IsMouseOverElement(typeof(DataGridCell)))
            {
                BeginEditRegisterWriteConfig();
            }
        }

        #endregion

        #endregion

        #region IWin32Window implementation

        public IntPtr Handle
        {
            get
            {
                return (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
            }
        }

        #endregion
    }
}
