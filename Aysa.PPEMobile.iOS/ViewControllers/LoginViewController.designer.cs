// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers
{
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UIView BlockedUserAlert { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField PasswordTextField { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint RailUserAlert { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField UserTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel VersionTextField { get; set; }

		[Action ("LoginButton_TouchUpInside:")]
		partial void LoginButton_TouchUpInside (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (BlockedUserAlert != null) {
				BlockedUserAlert.Dispose ();
				BlockedUserAlert = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (PasswordTextField != null) {
				PasswordTextField.Dispose ();
				PasswordTextField = null;
			}

			if (UserTextField != null) {
				UserTextField.Dispose ();
				UserTextField = null;
			}

			if (VersionTextField != null) {
				VersionTextField.Dispose ();
				VersionTextField = null;
			}

			if (RailUserAlert != null) {
				RailUserAlert.Dispose ();
				RailUserAlert = null;
			}
		}
	}
}
