using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Models.Responses;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.ServerMessages;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.ViewModels.Factories;
using FileCloud.Desktop.ViewModels.Interfaces;
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
        private readonly IUiDispatcher _dispatcher;
        private IMainViewModelFactory _mainVmFactory;
        private ILoginService _authService;
        private SyncService _syncService;
        private readonly MessageBus _bus;

        // ----------------------
        // Данные для UI
        // ----------------------

        public delegate void LoginHandler(MainViewModel mainVM);
        public event LoginHandler LoginSuccessful;

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set {  _login = value; OnPropertyChanged(); }
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
        private ServerStatus _serverStatus;
        public ServerStatus ServerStatus
        {
            get => _serverStatus;
            set { _serverStatus = value; OnPropertyChanged(); }
        }
        private string _serverStatusMessage;
        public string ServerStatusMessage
        {
            get => _serverStatusMessage;
            set { _serverStatusMessage = value; OnPropertyChanged(); }
        }
        private bool _isRegisterMode = false;

        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set { _isRegisterMode = value; OnPropertyChanged(); UpdateUI(); }
        }

        public string WindowTitle => IsRegisterMode ? "Регистрация" : "Вход в систему";
        public string ActionButtonText => IsRegisterMode ? "Создать аккаунт" : "Войти";
        public string SwitchModeText => IsRegisterMode ? "Войти в аккаунт" : "Создать аккаунт";

        // ----------------------
        // Команды для UI
        // ----------------------
        public ICommand RegisterCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand ActionCommand => IsRegisterMode ? RegisterCommand : LoginCommand;
        public ICommand SwitchModeCommand { get; }
        public LoginViewModel(ILoginService authService, SyncService syncService, IMainViewModelFactory mainVmFactory, IUiDispatcher dispatcher, MessageBus bus)
        {
            this._bus = bus;
            this._dispatcher = dispatcher;
            this._mainVmFactory = mainVmFactory;
            this._authService = authService;
            this._syncService = syncService;

            LoginCommand = new RelayCommand(async _ => await Authorization());
            RegisterCommand = new RelayCommand(async _ => await Registration());
            SwitchModeCommand = new RelayCommand(_ => IsRegisterMode = !IsRegisterMode);

            _bus.Subscribe<ServerIsActiveMessage>(async msg => await OnServerStateChanged(msg));
        }
        private void UpdateUI()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(SwitchModeText));
            OnPropertyChanged(nameof(ActionCommand));
        }

        private async Task Authorization()
        {
            try
            {
                var authResponse = await _authService.Login(Login, Password);
                OpenMainVM(authResponse);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task Registration()
        {
            try
            {
                var regResponse = await _authService.Register(Login, Password, Email);
                OpenMainVM(regResponse);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void OpenMainVM(AuthResponse authResponse)
        {
            var authSession = new AuthSessionModel(
                    authResponse.UserId,
                    authResponse.Login,
                    authResponse.Email,
                    authResponse.Token,
                    authResponse.RootFolder);

            var mainVm = _mainVmFactory.Create(authSession);
            LoginSuccessful?.Invoke(mainVm);
        }

        private async Task OnServerStateChanged(ServerIsActiveMessage message)
        {
            if(message.Status == ServerStatus.Offline)
            {
                await _syncService.StartMonitoringAsync();    
            }

            _dispatcher.BeginInvoke(() => ServerStatus = message.Status);
            _dispatcher.BeginInvoke(() => ServerStatusMessage = message.Message);
        }
    }
}
