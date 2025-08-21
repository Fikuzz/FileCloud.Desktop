using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FileCloud.Desktop.ViewModels.Helpers
{
    public class PreviewHelper
    {
        private readonly List<string> _previewExt = new List<string>() {".jpg",".jpeg",".png", ".mp4", ".mov", ".mkv" };
        private readonly string _icoPath = "/Assets/Icons";
        private readonly FileService _fileService;

        public PreviewHelper(IAppSettingsService settings, FileService fileService)
        {
            _icoPath = Path.Combine(settings.AppBasePath, "Assets", "Icons");
            _fileService = fileService;
        }
        public async Task SetPreview(ItemViewModel itemViewModel)
        {
            switch(itemViewModel)
            {
                case FileViewModel fileViewModel:
                    var ext = Path.GetExtension(fileViewModel.Name);
                    if (_previewExt.Contains(ext.ToLower()))
                    {
                        try {
                            fileViewModel.PreviewImage = await _fileService.GetPreviewAsync(fileViewModel.Id);
                        }catch (Exception ex)
                        {
                            SetLocalPreview(fileViewModel);
                            throw ex;
                        }
                    }
                    else
                        SetLocalPreview(fileViewModel);
                    return;
                case FolderViewModel folderViewModel:
                    folderViewModel.PreviewPath = Path.Combine(_icoPath, "folder.png");
                    return;
                default:
                    SetLocalPreview(itemViewModel);
                    return;
            }
        }

        private void SetLocalPreview(ItemViewModel itemViewModel)
        {
            var ext = Path.GetExtension(itemViewModel.Name)?.TrimStart('.').ToLowerInvariant() ?? ""; ;
            var icoPath = Path.Combine(_icoPath, $"{ext}.png");

            if (Path.Exists(icoPath))
            {
                itemViewModel.PreviewPath = icoPath;
            }
            else
            {
                itemViewModel.PreviewPath = Path.Combine(_icoPath, "default.png");
            }
        }
    }
}
