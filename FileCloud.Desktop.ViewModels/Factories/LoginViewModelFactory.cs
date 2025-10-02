using FileCloud.Desktop.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels.Factories
{
    public class LoginViewModelFactory : ILoginViewModelFactory
    {
        private readonly IServiceProvider _sp;
        public LoginViewModelFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public LoginViewModel Create()
        {
            var loginVM = _sp.GetRequiredService<LoginViewModel>();
            loginVM.StatusMessage = string.Empty;
            loginVM.Password = string.Empty;
            loginVM.Email = string.Empty;
            if (loginVM.IsRegisterMode)
                loginVM.SwitchModeCommand?.Execute(null);
            return loginVM;
        }
    }
}
