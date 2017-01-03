using System;
using SQLite;

namespace Imagician
{
	public interface ISQLite
	{
		SQLiteConnection GetConnection();
	}
}
