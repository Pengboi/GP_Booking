/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Appointment_Mgr"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Appointment_Mgr.Dialog;
using Appointment_Mgr.Dialog.OTP;
using GalaSoft.MvvmLight.Messaging;
//using Microsoft.Practices.ServiceLocation;

namespace Appointment_Mgr.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            
            SimpleIoc.Default.Register<MainViewModel>();


            SimpleIoc.Default.Register<HomeToolbarViewModel>();
            SimpleIoc.Default.Register<ReceptionistToolbarViewModel>();

            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public static LoginViewModel Login 
        {
            get { return ServiceLocator.Current.GetInstance<LoginViewModel>(); }
        }

        public static HomeViewModel Home
        {
            get { return ServiceLocator.Current.GetInstance<HomeViewModel>(); }
        }

        public static HomeToolbarViewModel HomeToolbar 
        {
            get { return ServiceLocator.Current.GetInstance<HomeToolbarViewModel>(); }
        }

        public static ReceptionistToolbarViewModel ReceptionistToolbar 
        {
            get { return ServiceLocator.Current.GetInstance<ReceptionistToolbarViewModel>(); }
        }


        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<MainViewModel>();


            SimpleIoc.Default.Register<HomeToolbarViewModel>();
            SimpleIoc.Default.Register<ReceptionistToolbarViewModel>();

            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();

            //Messenger.Reset();

        }
    }
}