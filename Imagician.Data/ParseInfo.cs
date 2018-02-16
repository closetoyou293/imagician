using System;
namespace Imagician
{
	public class ParseInfo
	{
		public int Id { get; internal set; }

		public string FolderPath
		{
			get;
			set;
		}

		public DateTime FolderModifiedDate
		{
			get;
			set;
		}

		public DateTime ParsingStartDate
		{
			get;
			set;
		}

		public DateTime ParsingFinishDate
		{
			get;
			set;
		}

		public int ChildFolderCount
		{
			get;
			set;
		}

		public int ChildImagesCount
		{
			get;
			set;
		}

		public int ChildImagesProcessedCount
		{
			get;
			set;
		}

		public int ChildFilesSkippedCount
		{
			get;
			set;
		}
	}

}
