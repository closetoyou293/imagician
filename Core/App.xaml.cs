using Imagician.Core.Services.Interfaces;
using Xamarin.Forms;

namespace Imagician
{
	public partial class App : Application
	{
		public static ImagicianDatabaseContext DBContext;
		public App(string dbPath)
		{
			DBContext = new ImagicianDatabaseContext(dbPath);
			DBContext.Database.EnsureDeleted();
			DBContext.Database.EnsureCreated();

			InitializeComponent();
			DependencyService.Register<IDatabaseService, ImagicianService>();
			DependencyService.Register<ILogService, LogService>();
			DependencyService.Register<ISettingsService, SettingsService>();
			DependencyService.Register<IFolderService, FolderItemService>();
			DependencyService.Register<IImageService, ImageService>();
			var root = new MasterDetailPage()
			{
				BindingContext = new ImagicianPageViewModel(),
				Master = new ImagicianPage(),
				Detail = new ImageDetailPage()
			};
			//MainPage = new NavigationPage(new FolderDetailPage()) { BindingContext = new ImagicianPageViewModel() };
			MainPage = root;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
