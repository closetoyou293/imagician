using System;
namespace Imagician
{
	public class ImageInfo
	{
		public int Id { get; internal set; }

		public string OldPath
		{
			get;
			set;
		}

		public string NewPath
		{
			get;
			set;
		}

		public byte[] Identity
		{
			get;
			set;
		}

		public bool WasProcessed
		{
			get;
			set;
		}

		public FolderItem OldParent
		{
			get;
			set;
		}

		public DateTime DateParsed
		{
			get;
			set;
		}

		public DateTime DateTaken
		{
			get;
			set;
		}

		public DateTime DateModified
		{
			get;
			set;
		}

		public DateTime FinalModified
		{
			get;
			set;
		}


		public string ExifInfo
		{
			get;
			set;
		}
	
	}
}
