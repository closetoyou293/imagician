using System;
using System.Collections.ObjectModel;

namespace Imagician
{
	public class LogService : ILogService
	{
		public LogService()
		{
		}

		public void AddMessage(string message)
		{
			Messages.Insert(0, $"{DateTime.Now.ToString()} ::: {message}");
		}

		public void AddMessage(string category, string message)
		{
			AddMessage($"{category} ::: {message}");
		}

		ObservableCollection<string> _messages = new ObservableCollection<string>();
		public ObservableCollection<string> Messages
		{
			get { return _messages; }
		}
	}
}
