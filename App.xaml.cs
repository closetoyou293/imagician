using Xamarin.Forms;

namespace Imagician
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

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
