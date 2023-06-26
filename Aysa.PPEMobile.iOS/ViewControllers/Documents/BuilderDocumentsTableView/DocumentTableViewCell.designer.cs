// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Aysa.PPEMobile.iOS.ViewControllers.Documents.BuilderDocumentsTableView
{
    [Register ("DocumentTableViewCell")]
    partial class DocumentTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DocumentTitleLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DocumentTitleLabel != null) {
                DocumentTitleLabel.Dispose ();
                DocumentTitleLabel = null;
            }
        }
    }
}