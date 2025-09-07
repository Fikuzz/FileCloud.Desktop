using FileCloud.Desktop.ViewModels;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileCloud.Desktop.View
{
    /// <summary>
    /// Логика взаимодействия для FileItemView.xaml
    /// </summary>
    public partial class FileItemView : UserControl
    {
        public FileItemView()
        {
            InitializeComponent();
        }
        private void OnPreviewLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            while (source is ContentElement)
                source = LogicalTreeHelper.GetParent(source);

            while (source != null && !(source is ListBoxItem))
                source = VisualTreeHelper.GetParent(source);

            var lbi = source as ListBoxItem;

            if (lbi != null && lbi.IsSelected)
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
    }
}
