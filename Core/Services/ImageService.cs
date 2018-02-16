using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ExifLib;
using Imagician.Core.Services.Interfaces;
using Xamarin.Forms;
using System.Linq;

namespace Imagician
{
	public class ImageService : IImageService
	{
		IFileService _fileService;
		IFolderService _folderService;
		ILogService _logService;
		ISettingsService _settingsService;
		IDatabaseService _service;

		string _exifDateFormat = "yyyy:MM:dd HH:mm:ss";

		string _backupPath => _settingsService.GetSetting<string>(SettingsService.BackupPath);

		string _noExifBaseFolder => _settingsService.GetSetting<string>(SettingsService.NoExifBaseFolder);

		string _pathToReplace => _settingsService.GetSetting<string>(SettingsService.PathToReplace);

		bool _createFolderWithYear = true;
		bool _createFolderWithMonth = true;
		bool _createFolderWithDay = false;

		bool _shouldReplacePath = true;

		bool _shouldGetPreviousPartsTillDigit = false;
		bool _adjustFirstDayOftheMonthPicturesToPreviousMonth = true;

		int _firstDayOfTheMonthTimeSpan = 7;

		public ImageService()
		{
			_service = DependencyService.Get<IDatabaseService>();
			_logService = DependencyService.Get<ILogService>();
			_fileService = DependencyService.Get<IFileService>();
			_folderService = DependencyService.Get<IFolderService>();
			_settingsService = DependencyService.Get<ISettingsService>();
		}

		public async Task<int> ParseFolderForImagesAsync(string folderPath, int folderProcessed, bool isRecursive)
		{

			_logService.AddMessage($"Parsing folder {folderPath}");
			var parseInfo = new ParseInfo();

			var items = _folderService.GetFilesForPath(folderPath);
			parseInfo.ParsingStartDate = DateTime.Now;
			parseInfo.ChildImagesCount = items.Where(c => c.IsImage).Count();
			parseInfo.ChildFolderCount = items.Where(c => c.IsFolder).Count();
			parseInfo.FolderPath = folderPath;
			await _service.AddParseInfoAsync(parseInfo);
			var imageProcessed = 0;
			folderProcessed += 1;
			foreach (var item in items)
			{
				if (item.IsImage)
					imageProcessed = await ProcessImage(imageProcessed, item);
				if (item.IsFolder && isRecursive)
					folderProcessed = await ParseFolderForImagesAsync(item.Path, folderProcessed, isRecursive);
				if (!item.IsImage && !item.IsFolder)
				{
					if (item.Path.StartsWith("$", StringComparison.OrdinalIgnoreCase))
						continue;
					var filePath = item.Path;
					var newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, filePath.Replace(_pathToReplace, ""));
					//  CopyFile(filePath, newFilePath, false);
				}
			}
			parseInfo.ChildImagesProcessedCount = imageProcessed;
			parseInfo.ParsingFinishDate = DateTime.Now;
			await _service.SaveChangesAsync();
			return folderProcessed;
		}

		async Task<int> ProcessImage(int imageProcessed, FolderItem item)
		{
			try
			{
				var imageInfo = new ImageInfo();
				imageInfo.OldPath = item.Path;
				_logService.AddMessage($"Processing image folder {imageInfo.OldPath}");
				imageInfo = CopyImage(imageInfo);
				await _service.AddImageInfoAsync(imageInfo);
				imageProcessed += 1;
			}
			catch (Exception ex)
			{

			}

			return imageProcessed;
		}

		ImageInfo CopyImage(ImageInfo dbInfo)
		{
			string filePath = dbInfo.OldPath;
			JpegInfo imageInfo = null;
			string newFilePath = "";
			bool usedExif = false;

			using (var stream = _fileService.GetFileStream(filePath))
			{
				imageInfo = ExifReader.ReadJpeg(stream);
			}

			if (imageInfo != null && (!string.IsNullOrEmpty(imageInfo.DateTime) || !string.IsNullOrEmpty(imageInfo.DateTimeOriginal)))
			{
				if (!string.IsNullOrEmpty(imageInfo.DateTimeOriginal))
				{
					var dtOriginal = DateTime.ParseExact(imageInfo.DateTimeOriginal, _exifDateFormat, CultureInfo.InvariantCulture);
					dbInfo.DateTaken = dtOriginal;
				}

				if (!string.IsNullOrEmpty(imageInfo.DateTime))
				{
					var dtOriginal = DateTime.ParseExact(imageInfo.DateTime, _exifDateFormat, CultureInfo.InvariantCulture);
					dbInfo.DateModified = dtOriginal;
				}

				var realDate = imageInfo.DateTimeOriginal ?? imageInfo.DateTime;
				dbInfo.FinalModified = ParseDate(realDate);
				usedExif = true;
			}
			else
			{
				var attrs = File.GetAttributes(filePath);
				var dtCreation = File.GetCreationTimeUtc(filePath);
				dbInfo.DateModified = dtCreation;
				dbInfo.FinalModified = ParseDate(dtCreation.ToString(_exifDateFormat));

				//newFilePath = Path.Combine(_backupPath, _noExifBaseFolder, _shouldReplacePath ? filePath.Replace(_pathToReplace, "") : filePath);
			}
			newFilePath = GetFilePathFromDate(filePath, newFilePath, dbInfo.FinalModified);
			newFilePath = Path.Combine(newFilePath, Path.GetFileName(filePath));
			dbInfo.ExifInfo = imageInfo.ToString();
			dbInfo.DateParsed = DateTime.Now;
			dbInfo.NewPath = newFilePath;
			CopyFile(filePath, newFilePath, usedExif);
			return dbInfo;
		}

		string GetFilePathFromDate(string filePath, string newFilePath, DateTime dt)
		{
			newFilePath = Path.Combine(_backupPath, newFilePath);
			if (_createFolderWithYear)
				newFilePath = Path.Combine(newFilePath, dt.Year.ToString());
			if (_createFolderWithMonth)
				newFilePath = Path.Combine(newFilePath, dt.Month.ToString("d2"));
			if (_createFolderWithDay)
				newFilePath = Path.Combine(newFilePath, dt.ToString("d2"));

			if (_shouldGetPreviousPartsTillDigit)
				newFilePath = GetPreviousPaths(ref newFilePath, filePath);
			return newFilePath;
		}

		DateTime ParseDate(string realDate)
		{
			
			var dt = DateTime.ParseExact(realDate, _exifDateFormat, CultureInfo.InvariantCulture);
			if (_adjustFirstDayOftheMonthPicturesToPreviousMonth && dt.Day == 1 && (dt.Hour >= 0 && dt.Hour <= _firstDayOfTheMonthTimeSpan))
			{
				dt = dt.AddDays(-1);
			}

			return dt;
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
						var fileName = Path.GetFileName(newFilePath);
						var name = Path.GetFileName(Path.GetDirectoryName(filePath));
						newFilePath = newFilePath.Replace(fileName, Path.Combine(name,fileName));
						CopyFile(filePath,newFilePath,usedExif);
						return;
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
