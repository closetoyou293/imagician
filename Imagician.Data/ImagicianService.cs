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
			_context = new ImagicianDatabaseContext();
		}

		public async Task<IEnumerable<FolderItem>> GetParsedFoldersAsync()
		{
			return _context.Folders.Where(c=> c.Done).ToAsyncEnumerable();
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
