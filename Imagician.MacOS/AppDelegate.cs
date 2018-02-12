﻿using System;
using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Imagician.MacOS
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{

		NSWindow _window;
		public AppDelegate()
		{
			ObjCRuntime.Runtime.MarshalManagedException += (sender, args) =>
			{
				Console.WriteLine(args.Exception.ToString());
			};

			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;


			var rect = new CoreGraphics.CGRect(200, 1000, 1000, 1000);
			//var rect = NSWindow.FrameRectFor(NSScreen.MainScreen.Frame, style);
			_window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			_window.Title = "Imagician";
			_window.TitleVisibility = NSWindowTitleVisibility.Hidden;

		}

		public override NSWindow MainWindow
		{
			get { return _window; }
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
		//	SQLitePCL.Batteries.Init();
			Forms.Init();
			DependencyService.Register<IFileService, FileService>();
			var app = new App();

			LoadApplication(app);
			base.DidFinishLaunching(notification);
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
