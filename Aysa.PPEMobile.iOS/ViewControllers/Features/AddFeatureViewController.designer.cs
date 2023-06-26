// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
	[Register ("AddFeatureViewController")]
	partial class AddFeatureViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView AttachmentContentView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField AutorTextField { get; set; }

		[Outlet]
		UIKit.UILabel CounterDetailText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton CreateButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField DateTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint HeightAttachmentContentConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField SectionEventTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView TitleTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint TopAttachmentContentConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint TopCreateButtonConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton UploadFileButton { get; set; }

		[Action ("CreateEvent:")]
		partial void CreateEvent (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AttachmentContentView != null) {
				AttachmentContentView.Dispose ();
				AttachmentContentView = null;
			}

			if (AutorTextField != null) {
				AutorTextField.Dispose ();
				AutorTextField = null;
			}

			if (CreateButton != null) {
				CreateButton.Dispose ();
				CreateButton = null;
			}

			if (DateTextField != null) {
				DateTextField.Dispose ();
				DateTextField = null;
			}

			if (HeightAttachmentContentConstraint != null) {
				HeightAttachmentContentConstraint.Dispose ();
				HeightAttachmentContentConstraint = null;
			}

			if (SectionEventTextField != null) {
				SectionEventTextField.Dispose ();
				SectionEventTextField = null;
			}

			if (TitleTextView != null) {
				TitleTextView.Dispose ();
				TitleTextView = null;
			}

			if (TopAttachmentContentConstraint != null) {
				TopAttachmentContentConstraint.Dispose ();
				TopAttachmentContentConstraint = null;
			}

			if (TopCreateButtonConstraint != null) {
				TopCreateButtonConstraint.Dispose ();
				TopCreateButtonConstraint = null;
			}

			if (UploadFileButton != null) {
				UploadFileButton.Dispose ();
				UploadFileButton = null;
			}

			if (CounterDetailText != null) {
				CounterDetailText.Dispose ();
				CounterDetailText = null;
			}
		}
	}
}
