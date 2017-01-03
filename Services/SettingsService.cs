using System;
using System.IO;

namespace Imagician
{
	public class SettingsService : ISettingsService
	{
		string _backupPath = Path.Combine("/");

		string _noExifBaseFolder = "nodate";

		string _pathToReplace = "/Volumes/data/Photos/";

		public const string BackupPath = "BackupPath";
		public const string NoExifBaseFolder = "NoExifBaseFolder";
		public const string PathToReplace = "PathToReplace";

		public T GetSetting<T>(string key)
		{
			switch (key)
			{
				case BackupPath:
					return (T)Convert.ChangeType(_backupPath, typeof(T));
				case NoExifBaseFolder:
					return (T)Convert.ChangeType(_noExifBaseFolder, typeof(T));
				case PathToReplace:
					return (T)Convert.ChangeType(_pathToReplace, typeof(T));
				default:
					break;
			}
			return default(T);
		}

		public void SetSetting<T>(string key, T value)
		{
			switch (key)
			{
				case BackupPath:
					_backupPath = value as string;
					break;
				case NoExifBaseFolder:
					_noExifBaseFolder = value as string;
					break;
				case PathToReplace:
					_pathToReplace = value as string;
					break;
				default:
					break;
			}
		}
	}
}
