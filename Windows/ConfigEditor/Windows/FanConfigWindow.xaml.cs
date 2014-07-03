using ConfigEditor.ViewModels;
using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using Xceed.Wpf.Toolkit;

namespace ConfigEditor.Windows
{
    /// <summary>
    /// Interaction logic for FanConfigWindow.xaml
    /// </summary>
    public partial class FanConfigWindow : NonModalDialogWindow
    {
        #region Constructors

        public FanConfigWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        #region Temperature Thresholds

        private void addThreshold_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as FanConfigViewModel;

            if (dataContext != null)
            {
                var viewModel = new TemperatureThresholdViewModel()
                {
                    Parent = dataContext
                };

                var dialog = new TemperatureThresholdWindow()
                {
                    DataContext = viewModel,
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    dataContext.TemperatureThresholds.Add(viewModel);
                }
            }
        }

        private void editThreshold_Click(object sender, RoutedEventArgs e)
        {
            BeginEditThreshold();
        }

        private void BeginEditThreshold()
        {
            var dataContext = this.DataContext as FanConfigViewModel;
            var viewModel = this.thresholdsGrid.SelectedValue as TemperatureThresholdViewModel;

            if (viewModel != null && dataContext != null)
            {
                var clonedViewModel = viewModel.Clone() as TemperatureThresholdViewModel;
                clonedViewModel.Parent = dataContext;

                var dialog = new TemperatureThresholdWindow()
                {
                    DataContext = clonedViewModel,
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    dataContext.TemperatureThresholds.Replace(viewModel, clonedViewModel);
                }
            }
        }

        private void removeThreshold_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as FanConfigViewModel;
            var viewModel = this.thresholdsGrid.SelectedValue as TemperatureThresholdViewModel;

            if (viewModel != null && dataContext != null)
            {
                dataContext.TemperatureThresholds.Remove(viewModel);
            }
        }

        private void loadDefaultThresholds_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as FanConfigViewModel;

            if (dataContext != null)
            {
                dataContext.TemperatureThresholds = new ObservableCollection<TemperatureThresholdViewModel>(
                    FanConfiguration.DefaultTemperatureThresholds.Select(x => new TemperatureThresholdViewModel()
                    {
                        DownThreshold = x.DownThreshold,
                        UpThreshold = x.UpThreshold,
                        FanSpeedPercentage = x.FanSpeed
                    }));
            }
        }

        private void thresholdsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.IsMouseOverElement(typeof(DataGridCell)))
            {
                BeginEditThreshold();
            }
        }

        #endregion

        #region Fan Speed Overrides

        private void addOverride_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as FanConfigViewModel;

            if (dataContext != null)
            {
                var viewModel = new FanSpeedOverrideViewModel()
                {
                    Parent = dataContext
                };

                var dialog = new FanSpeedOverrideWindow()
                {
                    DataContext = viewModel,
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    dataContext.FanSpeedOverrides.Add(viewModel);
                }
            }
        }

        private void editOverride_Click(object sender, RoutedEventArgs e)
        {
            BeginEditOverride();
        }

        private void removeOverride_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as FanConfigViewModel;
            var viewModel = this.overridesGrid.SelectedValue as FanSpeedOverrideViewModel;

            if (viewModel != null && dataContext != null)
            {
                dataContext.FanSpeedOverrides.Remove(viewModel);
            }
        }

        private void overridesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.IsMouseOverElement(typeof(DataGridCell)))
            {
                BeginEditOverride();
            }
        }

        private void BeginEditOverride()
        {
            var dataContext = this.DataContext as FanConfigViewModel;
            var viewModel = this.overridesGrid.SelectedValue as FanSpeedOverrideViewModel;

            if (viewModel != null && dataContext != null)
            {
                var clonedViewModel = viewModel.Clone() as FanSpeedOverrideViewModel;
                clonedViewModel.Parent = dataContext;

                var dialog = new FanSpeedOverrideWindow()
                {
                    DataContext = clonedViewModel,
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    dataContext.FanSpeedOverrides.Replace(viewModel, clonedViewModel);
                }
            }
        }

        #endregion

        #region Buttons

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

        #endregion
    }
}
