using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imagician.Core.Services.Interfaces
{
	public interface IDatabaseService
	{
		Task<int> SaveChangesAsync();
		Task<List<FolderItem>> GetFilesForFolderAsync(FolderItem parent);
		Task<List<FolderItem>> GetFoldersAsync();
		Task<List<FolderItem>> GetParsedFoldersAsync();
		Task AddFolderAsync(FolderItem folderitem);
		Task AddFileAsync(FolderItem folderitem, FolderItem file);

		Task AddParseInfoAsync(ParseInfo info);
		Task AddImageInfoAsync(ImageInfo info);
	}
}
