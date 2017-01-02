using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Imagician
{
	public partial class ImageDetailPage : ContentPage
	{
		public ImageDetailPage()
		{
			InitializeComponent();

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//	info.TranslateTo(0, 0);
		}
	}
}
