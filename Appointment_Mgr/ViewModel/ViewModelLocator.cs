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

            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<HomeToolbarViewModel>();

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


        public static ReceptionistHomeViewModel ReceptionistHome 
        {
            get { return ServiceLocator.Current.GetInstance<ReceptionistHomeViewModel>(); }
        }
        public static ReceptionistToolbarViewModel ReceptionistToolbar 
        {
            get { return ServiceLocator.Current.GetInstance<ReceptionistToolbarViewModel>(); }
        }


        public static BookAppointmentViewModel BookAppointment
        {
            get { return ServiceLocator.Current.GetInstance<BookAppointmentViewModel>(); }
        }
        public static ReservationAppointmentViewModel ReservationAppointment 
        {
            get { return ServiceLocator.Current.GetInstance<ReservationAppointmentViewModel>(); }
        }
        public static WalkInAppointmentViewModel WalkInAppointment 
        {
            get { return ServiceLocator.Current.GetInstance<WalkInAppointmentViewModel>(); }
        }


        public static ManagePatientViewModel ManagePatient
        {
            get { return ServiceLocator.Current.GetInstance<ManagePatientViewModel>(); }
        }
        public static AddPatientViewModel AddPatient 
        {
            get  { return ServiceLocator.Current.GetInstance<AddPatientViewModel>(); }
        }
        public static EditPatientViewModel EditPatient
        {
            get { return ServiceLocator.Current.GetInstance<EditPatientViewModel>(); }
        }
        public static DeletePatientViewModel DeletePatient 
        {
            get { return ServiceLocator.Current.GetInstance<DeletePatientViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();

            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<HomeToolbarViewModel>();

            SimpleIoc.Default.Register<ReceptionistHomeViewModel>();
            SimpleIoc.Default.Register<ReceptionistToolbarViewModel>();

            SimpleIoc.Default.Register<BookAppointmentViewModel>();
            SimpleIoc.Default.Register<ReservationAppointmentViewModel>();
            SimpleIoc.Default.Register<WalkInAppointmentViewModel>();

            SimpleIoc.Default.Register<ManagePatientViewModel>();
            SimpleIoc.Default.Register<AddPatientViewModel>();
            SimpleIoc.Default.Register<EditPatientViewModel>();
            SimpleIoc.Default.Register<DeletePatientViewModel>();
        }
    }
}