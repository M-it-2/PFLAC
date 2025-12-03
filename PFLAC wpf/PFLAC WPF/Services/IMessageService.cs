using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public interface IMessageService
    {
        void ShowError(string message);
        void ShowInfo(string message);
        void ShowSuccess(string message);
    }
}
