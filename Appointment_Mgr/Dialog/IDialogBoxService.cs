﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public interface IDialogBoxService
    {
        string OpenDialog<T>(DialogBoxViewModelBase<T> viewModel);
    }

}
