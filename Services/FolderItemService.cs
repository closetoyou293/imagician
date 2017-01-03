using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace Imagician
{
	public class FolderItemService : IFolderService
	{
		IFileService _fileService;
		readonly string[] _imageExtensions = { "jpg", "png", "jpeg", "bmp" };
		readonly string[] _excludeExtensions = { "db" };

		public FolderItemService()
		{
			_fileService = DependencyService.Get<IFileService>();
		}

		public IList<FolderItem> GetFilesForPath(string path)
		{
			var result = new List<FolderItem>();
			var files = _fileService.GetFolders(path, true);
			foreach (var item in files)
			{
				var folder = new FolderItem { Path = item, Title = Path.GetFileNameWithoutExtension(item) };
				var extension = Path.GetExtension(item);
				if (string.IsNullOrEmpty(extension))
				{
					folder.IsFolder = true;
				}
				else
				{
					if (_imageExtensions.Any(ext => extension.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
					{
						folder.ImagePath = item;
						folder.IsImage = true;
					}
					else if (_excludeExtensions.Any(ext => extension.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					else
					{
						//Messages.Insert(0, $"Not known extension {extension}");
					}
				}
				result.Add(folder);
			}
			return result;
		}

	}
}
