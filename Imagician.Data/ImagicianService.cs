using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Imagician
{
	public class ImagicianService
	{
		ImagicianDatabaseContext _context;

		public ImagicianService()
		{
			_context = App.DBContext;
		}

		public Task<List<FolderItem>> GetParsedFoldersAsync()
		{
			return Task.Run<List<FolderItem>>(() => _context.Folders.ToList());
		}
		

		public async Task AddFolderAsync(FolderItem folderitem)
		{
			await  _context.Folders.AddAsync(folderitem);
			await _context.SaveChangesAsync();
		}

		public async Task AddFileAsync(FolderItem folderitem, FolderItem file)
		{
			file.Parent = folderitem;
			await _context.Files.AddAsync(file);
			await _context.SaveChangesAsync();
		}
	}
}
