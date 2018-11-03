using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using NbfcClient.Services;

namespace NbfcClient.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
            }
            else
            {
                SimpleIoc.Default.Register<IFanControlClient, FanControlClient>(true);
            }

            SimpleIoc.Default.Register<MainViewModel>(true);
            SimpleIoc.Default.Register<SelectConfigViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SelectConfigViewModel SelectConfig
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SelectConfigViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }
    }
}
