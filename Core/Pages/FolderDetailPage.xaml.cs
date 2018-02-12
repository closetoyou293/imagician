using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Imagician
{
	public partial class FolderDetailPage : ContentPage
	{
		public FolderDetailPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			var vm = BindingContext as ImagicianPageViewModel;
			vm.Items.CollectionChanged += (sender, e) =>
			{
				LoadItems();
			};
			LoadItems();
			base.OnAppearing();
		}

		void LoadItems()
		{

			var vm = BindingContext as ImagicianPageViewModel;
			layout.Children.Clear();
			foreach (var speaker in vm.Items)
			{
				if (!speaker.IsImage)
					continue;

				// configure cell for wrap
				var cell = new StackLayout
				{
					WidthRequest = 200,
					HeightRequest = 200,
					BackgroundColor = Color.FromRgb(222, 222, 222),
					Children = {
						new Image {Source = speaker.ImagePath,
							VerticalOptions = LayoutOptions.Start,
							//BackgroundColor = Color.Blue,
							WidthRequest=130,
							HeightRequest=130},
						new Label {Text = speaker.Title,
							FontSize = 9,
							LineBreakMode = LineBreakMode.TailTruncation,
							VerticalOptions = LayoutOptions.Start,
							HorizontalOptions = LayoutOptions.Center}
					}
				};

				// add touch handling to show next page
				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.CommandParameter = speaker;
				tapGestureRecognizer.Tapped += (sender, e) =>
				{
					vm.SelectedPath = ((TappedEventArgs)e).Parameter as FolderItem;
					Navigation.PushAsync(new ImageDetailPage());
				};
				cell.GestureRecognizers.Add(tapGestureRecognizer);

				// add to wrap layout
				layout.Children.Add(cell);
			}
		}
	}
}
