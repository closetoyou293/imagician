using Xamarin.Forms;

namespace Imagician
{
	public partial class App : Application
	{
		public App()
		{
			using (var context = new ImagicianDatabaseContext())
			{

				//The line below clears and resets the databse.
				context.Database.EnsureDeleted();

				// Create the database if it does not exist
				context.Database.EnsureCreated();
			}

			InitializeComponent();
			DependencyService.Register<ILogService, LogService>();
			DependencyService.Register<ISettingsService, SettingsService>();
			DependencyService.Register<IFolderService, FolderItemService>();
			DependencyService.Register<IImageService, ImageService>();
			var root = new MasterDetailPage()
			{
				BindingContext = new ImagicianPageViewModel(),
				Master = new ImagicianPage(),
				Detail = new ContentPage()
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
