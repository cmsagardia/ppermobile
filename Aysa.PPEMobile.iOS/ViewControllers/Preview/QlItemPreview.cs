using System;
using QuickLook;
using Foundation;

namespace Aysa.PPEMobile.iOS.ViewControllers.Preview
{
    public class QlItemPreview : QLPreviewItem
    {
		string _title;
		Uri _uri;

		public QlItemPreview(string title, Uri uri)
		{
			this._title = title;
			this._uri = uri;
		}

		public override string ItemTitle
		{
			get { return _title; }
		}

		public override NSUrl ItemUrl
		{
			get { return _uri; }
		}
    }
}
