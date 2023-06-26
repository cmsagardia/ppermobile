// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events
{
	[Register ("AddEventViewController")]
	partial class AddEventViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView AttachmentContentView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton checkButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView ConteinerObservationView { get; set; }

		[Outlet]
		UIKit.UILabel CounterDetailText { get; set; }

		[Outlet]
		UIKit.UILabel CounterPlaceText { get; set; }

		[Outlet]
		UIKit.UILabel CounterTitleText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton CreateButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField DateTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView DetailTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint HeightAttachmentContentConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView ObservationTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField PlaceTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField ReferenceTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField SectionEventTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView StatusConteinerView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UISegmentedControl StatusSegmentedControl { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField TagTextField { get; set; }

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
		UIKit.NSLayoutConstraint TopTagConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField TypeTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton UploadFileButton { get; set; }

		[Action ("checkButton_TouchUpInside:")]
		partial void checkButton_TouchUpInside (UIKit.UIButton sender);

		[Action ("CreateButton_TouchUpInside:")]
		partial void CreateButton_TouchUpInside (UIKit.UIButton sender);

		[Action ("CreateEvent:")]
		partial void CreateEvent (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AttachmentContentView != null) {
				AttachmentContentView.Dispose ();
				AttachmentContentView = null;
			}

			if (checkButton != null) {
				checkButton.Dispose ();
				checkButton = null;
			}

			if (ConteinerObservationView != null) {
				ConteinerObservationView.Dispose ();
				ConteinerObservationView = null;
			}

			if (CreateButton != null) {
				CreateButton.Dispose ();
				CreateButton = null;
			}

			if (DateTextField != null) {
				DateTextField.Dispose ();
				DateTextField = null;
			}

			if (DetailTextView != null) {
				DetailTextView.Dispose ();
				DetailTextView = null;
			}

			if (HeightAttachmentContentConstraint != null) {
				HeightAttachmentContentConstraint.Dispose ();
				HeightAttachmentContentConstraint = null;
			}

			if (ObservationTextView != null) {
				ObservationTextView.Dispose ();
				ObservationTextView = null;
			}

			if (PlaceTextField != null) {
				PlaceTextField.Dispose ();
				PlaceTextField = null;
			}

			if (ReferenceTextField != null) {
				ReferenceTextField.Dispose ();
				ReferenceTextField = null;
			}

			if (SectionEventTextField != null) {
				SectionEventTextField.Dispose ();
				SectionEventTextField = null;
			}

			if (StatusConteinerView != null) {
				StatusConteinerView.Dispose ();
				StatusConteinerView = null;
			}

			if (StatusSegmentedControl != null) {
				StatusSegmentedControl.Dispose ();
				StatusSegmentedControl = null;
			}

			if (TagTextField != null) {
				TagTextField.Dispose ();
				TagTextField = null;
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

			if (TopTagConstraint != null) {
				TopTagConstraint.Dispose ();
				TopTagConstraint = null;
			}

			if (TypeTextField != null) {
				TypeTextField.Dispose ();
				TypeTextField = null;
			}

			if (UploadFileButton != null) {
				UploadFileButton.Dispose ();
				UploadFileButton = null;
			}

			if (CounterTitleText != null) {
				CounterTitleText.Dispose ();
				CounterTitleText = null;
			}

			if (CounterPlaceText != null) {
				CounterPlaceText.Dispose ();
				CounterPlaceText = null;
			}

			if (CounterDetailText != null) {
				CounterDetailText.Dispose ();
				CounterDetailText = null;
			}
		}
	}
}
