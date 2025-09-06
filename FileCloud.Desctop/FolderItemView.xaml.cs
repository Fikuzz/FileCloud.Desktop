using FileCloud.Desktop.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileCloud.Desktop.View
{
    /// <summary>
    /// Логика взаимодействия для FolderItemView.xaml
    /// </summary>
    public partial class FolderItemView : UserControl
    {
        public FolderItemView()
        {
            InitializeComponent();
        }

        private void FolderItem_MouseMove(object sender, MouseEventArgs e)
        {   
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mainWindow = Window.GetWindow(this);
                if (mainWindow?.DataContext is MainViewModel mainVm)
                {
                    var items = mainVm.SelectedItems;
                    if (items != null && items.Count > 0)
                    {
                        DataObject data = new DataObject();
                        data.SetData("FileCloudSelectedItemsFormat", items);

                        DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
                    }
                }
            }
        }

        private void Folder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Проверяем папки
            if (e.Data.GetDataPresent("FileCloudSelectedItemsFormat"))
            {
                if (DataContext is FolderViewModel targetFolder)
                {
                    var draggedItems = (IList<ItemViewModel>)e.Data.GetData("FileCloudSelectedItemsFormat");
                    foreach (ItemViewModel itemViewModel in draggedItems)
                    {
                        if (itemViewModel is FolderViewModel && itemViewModel.Id == targetFolder.Id)
                        {
                            return;
                        }
                    }
                }
                e.Effects = DragDropEffects.Move;
            }
        }

        private void Folder_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is FolderViewModel targetFolder)
            {
                // Обрабатываем файл
                if (e.Data.GetDataPresent("FileCloudSelectedItemsFormat"))
                {
                    var items = e.Data.GetData("FileCloudSelectedItemsFormat") as IList<ItemViewModel>;
                    if (items == null || items.Count == 0)
                        return;

                    foreach (ItemViewModel itemViewModel in items)
                    {
                        itemViewModel.Move(targetFolder.Id);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}
