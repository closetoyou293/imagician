using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imagician.MacOS
{
	public class FileService : IFileService
	{
		public List<string> GetFolders(string folder, bool showFiles = false)
		{
			var result = new List<string>();
			DirectoryInfo directory = new DirectoryInfo(folder);
			var directoryFiltered = directory.GetDirectories().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
			foreach (var item in directoryFiltered)
			{
				result.Add(item.FullName);
			}
			if (showFiles)
			{
				FileInfo[] files = directory.GetFiles();
				var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
				foreach (var item in filtered)
				{
					result.Add(item.FullName);
				}
			}

			Stopwatch stopwatch = new Stopwatch();

			// Begin timing.
			stopwatch.Start();

			//var size = DirSize(folder, true);

			// Stop timing.
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);

			return result;
		}

		public Stream GetFileStream(string filePath)
		{
			return File.Open(filePath, FileMode.Open);
		}

		private static long DirSize(string sourceDir, bool recurse)
		{
			DirectoryInfo di = new DirectoryInfo(sourceDir);
			if (di.Attributes.HasFlag(FileAttributes.Hidden))
				return 0;
			long size = 0;
			string[] fileEntries = Directory.GetFiles(sourceDir);

			foreach (string fileName in fileEntries)
			{
				Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
			}

			if (recurse)
			{
				string[] subdirEntries = Directory.GetDirectories(sourceDir);

				Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
				{
					if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
					{
						subtotal += DirSize(subdirEntries[i], true);
						return subtotal;
					}
					return 0;
				},
					(x) => Interlocked.Add(ref size, x)
				);
			}
			return size;
		}

		public static long GetFileSizeSumFromDirectory(string searchDirectory)
		{
			DirectoryInfo di = new DirectoryInfo(searchDirectory);
			if (di.Attributes.HasFlag(FileAttributes.Hidden))
				return 0;

			var files = Directory.EnumerateFiles(searchDirectory);

			// get the sizeof all files in the current directory
			var currentSize = (from file in files let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();

			var directories = Directory.EnumerateDirectories(searchDirectory);

			// get the size of all files in all subdirectories
			var subDirSize = (from directory in directories select GetFileSizeSumFromDirectory(directory)).Sum();

			return currentSize + subDirSize;
		}

		public List<string> GetImages()
		{
			var result = new List<string>();
			foreach (var item in Directory.GetDirectories("/"))
			{
				result.Add(item);
			}
			return result;
		}

		public string GetLocalFilePath(string filename)
		{
			string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string libFolder = Path.Combine(docFolder, "Library", "Databases");

			if (!Directory.Exists(libFolder))
			{
				Directory.CreateDirectory(libFolder);
			}

			return Path.Combine(libFolder, filename);
		}

		public void MoveFile(string filePath, string newFilePath)
		{
			new FileInfo(newFilePath).Directory.Create();
			File.Move(filePath, newFilePath);
		}

		public void CopyFile(string filePath, string newFilePath)
		{
			new FileInfo(newFilePath).Directory.Create();
			File.Copy(filePath, newFilePath);
		}

		public bool CompareFiles(string filePath, string newFilePath)
		{
			var fileInfo = new FileInfo(filePath);
			var newFileInfo = new FileInfo(newFilePath);
			return FilesAreEqual(fileInfo, newFileInfo);
		}

		static bool FilesAreEqual(FileInfo first, FileInfo second)
		{
			if (first.Length != second.Length)
				return false;

			int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

			using (FileStream fs1 = first.OpenRead())
			using (FileStream fs2 = second.OpenRead())
			{
				byte[] one = new byte[BYTES_TO_READ];
				byte[] two = new byte[BYTES_TO_READ];

				for (int i = 0; i < iterations; i++)
				{
					fs1.Read(one, 0, BYTES_TO_READ);
					fs2.Read(two, 0, BYTES_TO_READ);

					if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
						return false;
				}
			}

			return true;
		}
		const int BYTES_TO_READ = sizeof(Int64);
	}
}
