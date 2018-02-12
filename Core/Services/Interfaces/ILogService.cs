using System;
using System.Collections.ObjectModel;

namespace Imagician
{
	public interface ILogService
	{
		void AddMessage(string message);
		void AddMessage(string category, string message);
		ObservableCollection<string> Messages { get; }
	}
}
