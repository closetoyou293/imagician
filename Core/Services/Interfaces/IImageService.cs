using System.Threading.Tasks;

namespace Imagician
{
	public interface IImageService
	{
		Task ParseFolderForImagesAsync(string folderPath, bool isRecursive);
	}
}