using System;
using System.Collections.Generic;
using System.IO;

namespace Imagician
{
	public interface IFileService
	{
		bool CompareImages(string filePath, string newFilePath);

		void CopyFile(string filePath, string newFilePath);
		void MoveFile(string filePath, string newFilePath);
		Stream GetFileStream(string filePath);
		List<string> GetImages();
		List<string> GetFolders(string folder, bool showFiles = false);
		string GetLocalFilePath(string filename);
	}
}
