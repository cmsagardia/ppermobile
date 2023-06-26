// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderPeopleInCharge
{
    [Register ("PersonInChargeTableViewCell")]
    partial class PersonInChargeTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView containerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView IconImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NumberPhoneLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel PhoneTypeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView SeparatorView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (containerView != null) {
                containerView.Dispose ();
                containerView = null;
            }

            if (IconImage != null) {
                IconImage.Dispose ();
                IconImage = null;
            }

            if (NumberPhoneLabel != null) {
                NumberPhoneLabel.Dispose ();
                NumberPhoneLabel = null;
            }

            if (PhoneTypeLabel != null) {
                PhoneTypeLabel.Dispose ();
                PhoneTypeLabel = null;
            }

            if (SeparatorView != null) {
                SeparatorView.Dispose ();
                SeparatorView = null;
            }
        }
    }
}