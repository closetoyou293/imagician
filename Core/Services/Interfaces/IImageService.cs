using System.Threading.Tasks;

namespace Imagician
{
	public interface IImageService
	{
		Task<int> ParseFolderForImagesAsync(string folderPath, int folderParsed, bool isRecursive);
	}
}