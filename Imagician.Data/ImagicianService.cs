using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Imagician.Core.Services.Interfaces;

namespace Imagician
{
	public class ImagicianService : IDatabaseService
	{
		ImagicianDatabaseContext _context;

		public ImagicianService()
		{
			_context = App.DBContext;
		}

		public Task<int> SaveChangesAsync()
		{
			return _context.SaveChangesAsync();
		}

		public Task<List<FolderItem>> GetFilesForFolderAsync(FolderItem parent)
		{
			return Task.Run<List<FolderItem>>(() => _context.Files.Where(c=> c.Parent.Path == parent.Path).ToList());
		}


		public Task<List<FolderItem>> GetFoldersAsync()
		{
			return Task.Run<List<FolderItem>>(() => _context.Folders.ToList());
		}

		public Task<List<FolderItem>> GetParsedFoldersAsync()
		{
			return Task.Run<List<FolderItem>>(() => _context.Folders.Where(c=> c.Done).ToList());
		}
		

		public async Task AddFolderAsync(FolderItem folderitem)
		{
			if (_context.Folders.FirstOrDefault(c => c.Path == folderitem.Path) != null)
				return;
			await  _context.Folders.AddAsync(folderitem);
			await _context.SaveChangesAsync();
		}

		public async Task AddFileAsync(FolderItem folderitem, FolderItem file)
		{
			if (_context.Files.FirstOrDefault(c => c.Path == file.Path) != null)
				return;
			file.Parent = folderitem;
			await _context.Files.AddAsync(file);
			await _context.SaveChangesAsync();
		}

		public async Task AddParseInfoAsync(ParseInfo info)
		{
			if (_context.ParseInfos.FirstOrDefault(c => c.FolderPath == info.FolderPath) != null)
				return;
			await _context.ParseInfos.AddAsync(info);
			await _context.SaveChangesAsync();
		}

		public async Task AddImageInfoAsync(ImageInfo info)
		{
			if (_context.ParseImageInfos.FirstOrDefault(c => c.OldPath == info.OldPath) != null)
				return;
			await _context.ParseImageInfos.AddAsync(info);
			await _context.SaveChangesAsync();
		}

	}
}
