using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static LoginWindow LoginView 
        {
            get { return new LoginWindow(); }
        }

        public static ManagerWindow ManageView 
        {
            get { return new ManagerWindow(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.CurrentView.Content = new LoginWindow();

            Messenger.Default.Register<string>(this, changeUserControl);
        }

        private void changeUserControl(string msg)
        {
            if (msg == "HomeView")
                this.CurrentView.Content = LoginView;
            else if (msg == "GrantedView")
                this.CurrentView.Content = ManageView;
        }
    }
}
