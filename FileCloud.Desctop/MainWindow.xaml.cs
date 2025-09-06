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

                if (files == null || files.Length == 0)
                    return;

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

        private void Breadcrumb_DragOver(object sender, DragEventArgs e)
        {
            if (IsValidDropTarget(sender, e))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Breadcrumb_Drop(object sender, DragEventArgs e)
        {
            if (IsValidDropTarget(sender, e) &&
                sender is Button button &&
                button.DataContext is FolderViewModel targetFolder)
            {
                // Перемещаем файлы/папки
                if (e.Data.GetDataPresent("FileCloudSelectedItemsFormat"))
                {
                    var files = (IList<ItemViewModel>)e.Data.GetData("FileCloudSelectedItemsFormat");
                    foreach (var file in files)
                        file.Move(targetFolder.Id);
                }
            }
            e.Handled = true;
        }

        private bool IsValidDropTarget(object sender, DragEventArgs e)
        {
            // Проверяем что перетаскиваем наши файлы/папки
            if (!e.Data.GetDataPresent("FileCloudSelectedItemsFormat"))
                return false;

            // Проверяем что цель - папка
            if (sender is not Button button || button.DataContext is not FolderViewModel targetFolder)
                return false;

            return true;
        }
    }
}