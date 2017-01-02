using Xamarin.Forms;

namespace Imagician
{

	public class PersonDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ImageItemTemplate { get; set; }
		public DataTemplate NormalItemTemplate { get; set; }
		public DataTemplate FolderItemTemplate { get; set; }


		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return ((FolderItem)item).IsFolder ? FolderItemTemplate : ((FolderItem)item).IsImage ? ImageItemTemplate : NormalItemTemplate;
		}
	}

	public partial class ImagicianPage : ContentPage
	{
		ImagicianPageViewModel ViewModel
		{
			get { return BindingContext as ImagicianPageViewModel; }
		}
		public ImagicianPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Init();
		}

		void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
		{

		}
	}
}
