using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileCloud.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await ((MainViewModel)DataContext).HandleDroppedFiles(files);
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            // Проверяем, что перетаскиваются файлы
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
    }
}