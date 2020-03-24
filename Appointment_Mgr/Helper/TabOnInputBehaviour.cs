using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Appointment_Mgr.Helper
{
    public class TabOnInputBehavior : Behavior<TextBox>
    {

        protected override void OnAttached()
        {
            AssociatedObject.PreviewKeyUp += AssociatedObject_PreviewKeyUp;
        }

        private void AssociatedObject_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // tab forward on number press
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                AssociatedObject.MoveFocus(request);
            }

            // tab backwards on delete key press
            if (e.Key == Key.Back) 
            {
                var request = new TraversalRequest(FocusNavigationDirection.Previous);
                request.Wrapped = true;
                AssociatedObject.MoveFocus(request);
            }
            
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyUp;
        }

    }
}
