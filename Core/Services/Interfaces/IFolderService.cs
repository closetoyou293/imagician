using System.Collections.Generic;

namespace Imagician
{
	public interface IFolderService
	{
		IList<FolderItem> GetFilesForPath(string path);
	}
}