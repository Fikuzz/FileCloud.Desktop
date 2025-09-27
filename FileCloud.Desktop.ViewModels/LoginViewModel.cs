using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.ViewModels.Factories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileCloud.Desktop.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        // ----------------------
        // INotifyPropertyChanged
        // ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ----------------------
        // Сервисы через DI
        // ----------------------
        private IMainViewModelFactory _mainVmFactory;
        private ILoginService _authService;
        private SyncService _syncService;

        // ----------------------
        // Данные для UI
        // ----------------------

        public delegate void LoginHandler(MainViewModel mainVM);
        public event LoginHandler LoginSuccessful;

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set {  _login = value; Debug.WriteLine($"[VM] Login updated: '{_login}'"); OnPropertyChanged(); }
        }
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    System.Diagnostics.Debug.WriteLine($"[VM] Password updated: '{_password}'");
                    OnPropertyChanged();
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // ----------------------
        // Команды для UI
        // ----------------------
        public ICommand RegisterCommand { get; }
        public ICommand LoginCommand { get; }

        public LoginViewModel(ILoginService authService, SyncService syncService, IMainViewModelFactory mainVmFactory)
        {
            this._mainVmFactory = mainVmFactory;
            this._authService = authService;
            this._syncService = syncService;

            LoginCommand = new RelayCommand(async _ => await Authorization());
        }

        private async Task Authorization()
        {
            try
            {
                var authResponse = await _authService.Login(Login, Password);
                var authSession = new AuthSessionModel(
                    authResponse.UserId,
                    authResponse.Login,
                    authResponse.Email,
                    authResponse.Token,
                    authResponse.RootFolder);

                var mainVm = _mainVmFactory.Create(authSession);
                LoginSuccessful?.Invoke(mainVm);
                
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
    }
}
