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

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
    [Register ("FeatureDetailViewController")]
    partial class FeatureDetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView AttachmentContentView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AutorLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DateLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DetailLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem EditButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint HeightAttachmentConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField SectionTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TitleEventLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint TopAttachmentContentConstraint { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AttachmentContentView != null) {
                AttachmentContentView.Dispose ();
                AttachmentContentView = null;
            }

            if (AutorLabel != null) {
                AutorLabel.Dispose ();
                AutorLabel = null;
            }

            if (DateLabel != null) {
                DateLabel.Dispose ();
                DateLabel = null;
            }

            if (DetailLabel != null) {
                DetailLabel.Dispose ();
                DetailLabel = null;
            }

            if (EditButton != null) {
                EditButton.Dispose ();
                EditButton = null;
            }

            if (HeightAttachmentConstraint != null) {
                HeightAttachmentConstraint.Dispose ();
                HeightAttachmentConstraint = null;
            }

            if (SectionTextField != null) {
                SectionTextField.Dispose ();
                SectionTextField = null;
            }

            if (TitleEventLabel != null) {
                TitleEventLabel.Dispose ();
                TitleEventLabel = null;
            }

            if (TopAttachmentContentConstraint != null) {
                TopAttachmentContentConstraint.Dispose ();
                TopAttachmentContentConstraint = null;
            }
        }
    }
}