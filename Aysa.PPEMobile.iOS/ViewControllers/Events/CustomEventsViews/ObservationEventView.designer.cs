// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS
{
	[Register ("ObservationEventView")]
	partial class ObservationEventView
	{
		[Outlet]
		UIKit.UIView AttachmentContentView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel DateLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel NameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DateLabel != null) {
				DateLabel.Dispose ();
				DateLabel = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}

			if (AttachmentContentView != null) {
				AttachmentContentView.Dispose ();
				AttachmentContentView = null;
			}
		}
	}
}
