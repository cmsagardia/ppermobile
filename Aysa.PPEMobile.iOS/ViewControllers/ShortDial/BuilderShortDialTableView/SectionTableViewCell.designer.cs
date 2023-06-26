// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderShortDialTableView
{
	[Register ("SectionTableViewCell")]
	partial class SectionTableViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView containerView { get; set; }

		[Outlet]
		UIKit.UIButton FavoriteButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel NameLabel { get; set; }

		[Action ("ChangeFavorite:")]
		partial void ChangeFavorite (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (containerView != null) {
				containerView.Dispose ();
				containerView = null;
			}

			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}

			if (FavoriteButton != null) {
				FavoriteButton.Dispose ();
				FavoriteButton = null;
			}
		}
	}
}
