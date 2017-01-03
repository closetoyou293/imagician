using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLib;
using Xamarin.Forms;

namespace Imagician
{

	public class ImagicianPageViewModel : BaseViewModel
	{
		string _previousSelectedPath;
		IFileService _fileService;
		Stack<FolderItem> _nav = new Stack<FolderItem>();

		string _backupPath = Path.Combine("/", "Volumes", "BACKUP", "Fotos");

		string _noExifBaseFolder = "nodate";

		string _pathToReplate = "/Volumes/data/Photos/";

		string _exifDateFormat = "yyyy:MM:dd HH:mm:ss";

		string[] _imageExtensions = { "jpg", "png", "jpeg", "bmp" };

		string[] _excludeExtensions = { "db" };

		bool _createFolderWithYear = true;
		bool _createFolderWithMonth = true;
		bool _createFolderWithDay = false;

		bool _shouldReplacePath = true;

		bool _shouldGetPreviousPartsTillDigit = true;
		bool _adjustFirstDayOftheMonthPicturesToPreviousMonth = true;

		int _firstDayOfTheMonthTimeSpan = 5;

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

		Command _getImagesCommand;
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
						Messages.Insert(0, $"Not known extension {extension}");
					}
				}
				result.Add(folder);
			}
			return result;
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

			if (imageInfo != null && (!string.IsNullOrEmpty(imageInfo.DateTime) || !string.IsNullOrEmpty(imageInfo.DateTimeOriginal)))
			{
				var realDate = imageInfo.DateTimeOriginal ?? imageInfo.DateTime;

				var dt = DateTime.ParseExact(realDate, _exifDateFormat, CultureInfo.InvariantCulture);
				if (_adjustFirstDayOftheMonthPicturesToPreviousMonth && dt.Day == 1 && (dt.Hour >= 0 && dt.Hour <= _firstDayOfTheMonthTimeSpan))
				{
					dt = dt.AddDays(-1);
				}

				usedExif = true;
				newFilePath = Path.Combine(newFilePath, _backupPath);
				if (_createFolderWithYear)
					newFilePath = Path.Combine(newFilePath, dt.Year.ToString());
				if (_createFolderWithMonth)
					newFilePath = Path.Combine(newFilePath, dt.Month.ToString("d2"));
				if (_createFolderWithDay)
					newFilePath = Path.Combine(newFilePath, dt.ToString("d2"));

				if (_shouldGetPreviousPartsTillDigit)
					newFilePath = GetPreviousPaths(ref newFilePath, filePath);
				newFilePath = Path.Combine(newFilePath, Path.GetFileName(filePath));
			}
			else
			{
				newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, _shouldReplacePath ? filePath.Replace(_pathToReplate, "") : filePath);
			}
			CopyFile(filePath, newFilePath, usedExif);
		}

		void CopyFile(string filePath, string newFilePath, bool usedExif)
		{
			Messages.Insert(0, $"Copy {filePath} to {newFilePath} Used exif: {usedExif}");
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
					else
					{
					}
				}
				throw ex;

			}
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
	}
}
