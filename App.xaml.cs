using Xamarin.Forms;

namespace Imagician
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			DependencyService.Register<ILogService, LogService>();
			DependencyService.Register<IFolderService, FolderItemService>();
			DependencyService.Register<IImageService, ImageService>();
			var root = new MasterDetailPage();
			root.BindingContext = new ImagicianPageViewModel();
			root.Master = new ImagicianPage();
			root.Detail = new ImageDetailPage();

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
