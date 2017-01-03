using System;
namespace Imagician
{
	public interface ISettingsService
	{
		T GetSetting<T>(string key);
		void SetSetting<T>(string key, T value);
	}
}
