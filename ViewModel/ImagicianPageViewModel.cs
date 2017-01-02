using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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

	public class ImagicianPageViewModel : BaseViewModel
	{
		IFileService _fileService;
		Stack<FolderItem> _nav = new Stack<FolderItem>();

		string _backupPath = Path.Combine("/", "Volumes", "BACKUP", "Fotos");

		string _noExifBaseFolder = "nodate";

		string _pathToReplate = "/Volumes/data/Photos/";

		public ImagicianPageViewModel()
		{
			_fileService = DependencyService.Get<IFileService>();
			PropertyChanged += (sender, e) =>
			{

				if (e.PropertyName == "IsBusy")
					GetImagesCommand.ChangeCanExecute();
			};
		}


		public void Init()
		{
			//var database = await TodoItemDatabase.Database.GetItemsAsync();
			SelectedPath = new FolderItem { Title = "root", Path = Path.Combine("/", "Volumes", "data", "Photos"), IsFolder = true };

		}

		bool _isRecursive = true;

		public bool IsRecursive
		{
			get { return _isRecursive; }
			private set { SetProperty(ref _isRecursive, value); }
		}

		ObservableCollection<FolderItem> _items = new ObservableCollection<FolderItem>();

		public ObservableCollection<FolderItem> Items
		{
			get { return _items; }
			private set { SetProperty(ref _items, value); }
		}
		string _previousSelectedPath;

		FolderItem _selectedPath;

		public FolderItem SelectedPath
		{
			get { return _selectedPath; }
			set
			{
				if (_selectedPath == value)
					return;

				if (!value.IsFolder)
				{
					SelectedFile = value;
					return;
				}

				SetProperty(ref _selectedPath, value);
				GetFiles();
				if (_nav.Count == 0 || _nav.Peek() != _selectedPath)
					_nav.Push(_selectedPath);

				GoBackCommand.ChangeCanExecute();
				GetImagesCommand.ChangeCanExecute();
			}
		}

		FolderItem _selectedFile;
		public FolderItem SelectedFile
		{
			get { return _selectedFile; }
			set
			{
				if (_selectedFile == value)
					return;
				if (value.IsFolder)
					return;
				SetProperty(ref _selectedFile, value);
				if (!_selectedFile.IsImage)
					return;
				using (var stream = _fileService.GetFileStream(_selectedFile.ImagePath))
				{
					SelectedImageInfo = ExifReader.ReadJpeg(stream);

				}
			}
		}

		JpegInfo _selectedImageInfo;
		public JpegInfo SelectedImageInfo
		{
			get { return _selectedImageInfo; }
			set
			{
				if (_selectedImageInfo == value)
					return;

				SetProperty(ref _selectedImageInfo, value);

			}
		}

		ObservableCollection<string> _messages = new ObservableCollection<string>();

		public ObservableCollection<string> Messages
		{
			get { return _messages; }
			private set { SetProperty(ref _messages, value); }
		}

		Command _goBackCommand;

		Command _getImagesCommand;

		void GetFiles()
		{
			Items.Clear();
			foreach (var item in GetFilesForPath(SelectedPath.Path))
			{
				Items.Add(item);
			}
		}

		IList<FolderItem> GetFilesForPath(string path)
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
					if (extension.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || extension.EndsWith("png", StringComparison.OrdinalIgnoreCase) || extension.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
					{
						folder.ImagePath = item;
						folder.IsImage = true;
					}
					else
					{
						Messages.Insert(0, $"Not known extension {extension}");
					}
				}
				result.Add(folder);
			}
			return result;
		}


		public Command GoBackCommand
		{
			get
			{
				return _goBackCommand ?? (_goBackCommand = new Command((arg) =>
				{
					var s = _nav.Pop();
					SelectedPath = _nav.Peek();

				}, (arg) => _nav.Count > 1));
			}
		}

		public Command GetImagesCommand
		{
			get
			{
				return _getImagesCommand ?? (_getImagesCommand = new Command(async (arg) =>
				{
					IsBusy = true;
					var path = SelectedPath.Path;
					await ParseFolderForImagesAsync(path, IsRecursive);
					Messages.Insert(0, $"Ended Parsing Folder {path}");
					IsBusy = false;
				}, (arg) => SelectedPath != null && !IsBusy));
			}
		}

		Task ParseFolderForImagesAsync(string folderPath, bool isRecursive)
		{
			return Task.Run(async () =>
			   {
				   Messages.Insert(0, $"Parsing Folder {folderPath} using recursive:{isRecursive}");
				   var items = GetFilesForPath(folderPath);

				   foreach (var item in items)
				   {
					   if (item.IsImage)
						   CopyImage(item.ImagePath);
					   else if (!item.IsImage && !item.IsFolder)
					   {
						   var filePath = item.Path;
						   var newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, filePath.Replace(_pathToReplate, ""));
						   CopyFile(filePath, newFilePath, false);
					   }

					   if (item.IsFolder && isRecursive)
						   await ParseFolderForImagesAsync(item.Path, isRecursive);

				   }
			   });

		}

		void CopyImage(string filePath)
		{
			JpegInfo imageInfo = null;
			string newFilePath = "";
			bool usedExif = false;
			using (var stream = _fileService.GetFileStream(filePath))
			{
				imageInfo = ExifReader.ReadJpeg(stream);
			}

			if (imageInfo != null && !string.IsNullOrEmpty(imageInfo.DateTime))
			{
				var dt = DateTime.ParseExact(imageInfo.DateTime, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
				newFilePath = Path.Combine(_backupPath, dt.Year.ToString(), dt.Month.ToString());
				usedExif = true;

				newFilePath = GetPreviousPaths(ref newFilePath, filePath);

				newFilePath = Path.Combine(newFilePath, Path.GetFileName(filePath));
			}
			else
			{
				newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, filePath.Replace(_pathToReplate, ""));

			}
			CopyFile(filePath, newFilePath, usedExif);

		}

		static string GetPreviousPaths(ref string newFilePath, string filePath)
		{
			var pathPrevious = Path.GetFileName(Path.GetDirectoryName(filePath));
			if (!IsDigitsOnly(pathPrevious))
			{
				GetPreviousPaths(ref newFilePath, Path.GetDirectoryName(filePath));
				newFilePath = Path.Combine(newFilePath, pathPrevious);
			}

			return newFilePath;
		}

		static bool IsDigitsOnly(string str)
		{
			foreach (char c in str)
			{
				if (c < '0' || c > '9')
					return false;
			}

			return true;
		}

		void CopyFile(string filePath, string newFilePath, bool usedExif)
		{
			Messages.Insert(0, $"Try Copy {filePath} to {newFilePath} Used exif: {usedExif}");
			try
			{
				_fileService.CopyFile(filePath, newFilePath);
			}
			catch (IOException ex)
			{
				if (ex.Message.Contains("already exists"))
				{

					var compare = _fileService.CompareImages(filePath, newFilePath);
					Messages.Insert(0, $"File {newFilePath} already exists and is same {compare}");
					if (compare)
						return;
				}
				throw ex;

			}
		}
	}
}
