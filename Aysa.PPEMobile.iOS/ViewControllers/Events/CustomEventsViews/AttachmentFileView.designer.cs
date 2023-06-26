// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Aysa.PPEMobile.iOS
{
    [Register ("AttachmentFileView")]
    partial class AttachmentFileView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint HeightRemoveConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView IconImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NameFileLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton RemoveButton { get; set; }

        [Action ("UIButtonIsObV3X0_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButtonIsObV3X0_TouchUpInside (UIKit.UIButton sender);

        [Action ("UIButtonR2QvsM5n_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButtonR2QvsM5n_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (HeightRemoveConstraint != null) {
                HeightRemoveConstraint.Dispose ();
                HeightRemoveConstraint = null;
            }

            if (IconImageView != null) {
                IconImageView.Dispose ();
                IconImageView = null;
            }

            if (NameFileLabel != null) {
                NameFileLabel.Dispose ();
                NameFileLabel = null;
            }

            if (RemoveButton != null) {
                RemoveButton.Dispose ();
                RemoveButton = null;
            }
        }
    }
}