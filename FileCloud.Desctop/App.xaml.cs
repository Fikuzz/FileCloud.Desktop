using System.Configuration;
using System.Data;
using System.Windows;
using FileCloud.Desktop.Services;

namespace FileCloud.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ScriptManager.EnsureDefaultScripts();
        }
    }

}
