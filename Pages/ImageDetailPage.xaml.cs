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
			btnExif.Command = new Command((obj) =>
				 {
					 info.TranslateTo(info.TranslationX == 200 ? 0 : 200, 0);
				 });

			btnLog.Command = new Command((obj) =>
			{
				lstLog.TranslateTo(0, lstLog.TranslationY == -100 ? 0 : -100);
			});
		}
	}
}
