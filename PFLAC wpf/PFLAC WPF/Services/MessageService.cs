using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PFLAC_WPF.Services
{
    class MessageService: IMessageService
    {
        public void ShowError(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
