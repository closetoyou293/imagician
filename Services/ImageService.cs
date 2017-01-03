using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ExifLib;
using Xamarin.Forms;

namespace Imagician
{
	public class ImageService : IImageService
	{
		IFileService _fileService;
		IFolderService _folderService;
		ILogService _logService;

		string _backupPath = Path.Combine("/");

		string _noExifBaseFolder = "nodate";

		string _pathToReplace = "/Volumes/data/Photos/";

		string _exifDateFormat = "yyyy:MM:dd HH:mm:ss";

		bool _createFolderWithYear = true;
		bool _createFolderWithMonth = true;
		bool _createFolderWithDay = false;

		bool _shouldReplacePath = true;

		bool _shouldGetPreviousPartsTillDigit = true;
		bool _adjustFirstDayOftheMonthPicturesToPreviousMonth = true;

		int _firstDayOfTheMonthTimeSpan = 5;

		public ImageService()
		{
			_logService = DependencyService.Get<ILogService>();
			_fileService = DependencyService.Get<IFileService>();
			_folderService = DependencyService.Get<IFolderService>();
		}

		public Task ParseFolderForImagesAsync(string folderPath, bool isRecursive, string backupPath = null)
		{
			if (backupPath != null)
				_backupPath = backupPath;
			return Task.Run(async () =>
			   {
				   var items = _folderService.GetFilesForPath(folderPath);

				   foreach (var item in items)
				   {
					   if (item.IsImage)
						   CopyImage(item.ImagePath);
					   else if (!item.IsImage && !item.IsFolder)
					   {
						   var filePath = item.Path;
						   var newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, filePath.Replace(_pathToReplace, ""));
						   CopyFile(filePath, newFilePath, false);
					   }

					   if (item.IsFolder && isRecursive)
						   await ParseFolderForImagesAsync(item.Path, isRecursive, backupPath);

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
				newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, _shouldReplacePath ? filePath.Replace(_pathToReplace, "") : filePath);
			}
			CopyFile(filePath, newFilePath, usedExif);
		}

		void CopyFile(string filePath, string newFilePath, bool usedExif)
		{
			_logService.AddMessage(nameof(IImageService), $"Copy {filePath} to {newFilePath} Used exif: {usedExif}");
			try
			{
				_fileService.CopyFile(filePath, newFilePath);
			}
			catch (IOException ex)
			{
				if (ex.Message.Contains("already exists"))
				{

					var compare = _fileService.CompareFiles(filePath, newFilePath);
					_logService.AddMessage(nameof(IImageService), $"File {newFilePath} already exists and is same {compare}");
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
