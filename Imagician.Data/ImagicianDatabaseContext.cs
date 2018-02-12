using System;
using Microsoft.EntityFrameworkCore;

namespace Imagician
{
	public class ImagicianDatabaseContext : DbContext
	{
		public DbSet<FolderItem> Folders { get; set; }

		public DbSet<FolderItem> Files { get; set; }

		public ImagicianDatabaseContext()
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// Specify the path of the database here
			optionsBuilder.UseSqlite("Filename=./imagician.sqlite");
		}
	}
}
