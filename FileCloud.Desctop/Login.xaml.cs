using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.ViewModels;
using FileCloud.Desktop.ViewModels.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileCloud.Desktop.View
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login(LoginViewModel loginVM)
        {
            InitializeComponent();
            loginVM.LoginSuccessful += OnLoginSuccessful;
            DataContext = loginVM;
        }

        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileCloud.Desktop.ViewModels.LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void OnLoginSuccessful(MainViewModel mainVM)
        {
            MainWindow mainWindow = new MainWindow(mainVM);
            mainWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.LoginSuccessful -= OnLoginSuccessful;

            base.OnClosed(e);
        }
    }
}
