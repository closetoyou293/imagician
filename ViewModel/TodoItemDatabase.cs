using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLib;
using SQLite;
using Xamarin.Forms;

namespace Imagician
{

	public class TodoItemDatabase
	{
		static TodoItemDatabase database;

		static SQLiteAsyncConnection connection;

		public static TodoItemDatabase Database
		{
			get
			{
				if (database == null)
				{
					database = new TodoItemDatabase(DependencyService.Get<IFileService>().GetLocalFilePath("TodoSQLite.db3"));
				}
				return database;
			}
		}

		public TodoItemDatabase(string dbPath)
		{
			try
			{
				connection = new SQLiteAsyncConnection(dbPath);

				connection.CreateTableAsync<FolderItem>().Wait();
			}
			catch (Exception ex)
			{

			}

		}

		public Task<List<FolderItem>> GetItemsAsync()
		{
			return connection.Table<FolderItem>().ToListAsync();
		}

		public Task<List<FolderItem>> GetItemsNotDoneAsync()
		{
			return connection.QueryAsync<FolderItem>("SELECT * FROM [FolderItem] WHERE [Done] = 0");
		}

		public Task<FolderItem> GetItemAsync(int id)
		{
			return connection.Table<FolderItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
		}

		public Task<int> SaveItemAsync(FolderItem item)
		{
			if (item.ID != 0)
			{
				return connection.UpdateAsync(item);
			}
			else {
				return connection.InsertAsync(item);
			}
		}

		public Task<int> DeleteItemAsync(FolderItem item)
		{
			return connection.DeleteAsync(item);
		}
	}
	
}
