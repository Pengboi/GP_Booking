using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Helper
{
    public class GenderList : List<string>
    {
        public GenderList() 
        {
            this.Add("Male");
            this.Add("Female");
        }
    }
}
