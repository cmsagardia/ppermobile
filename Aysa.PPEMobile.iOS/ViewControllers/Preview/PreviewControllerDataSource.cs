using System;
using QuickLook;
using Foundation;
using UIKit;
using System.IO;

namespace Aysa.PPEMobile.iOS.ViewControllers.Preview
{
    public class PreviewControllerDataSource: QLPreviewControllerDataSource
    {
		private string fileName;

		public PreviewControllerDataSource(string fileName)
		{
			this.fileName = fileName;

		}

		public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
		{

			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var library = Path.Combine(documents, this.fileName);
			NSUrl url = NSUrl.FromFilename(library);
            return new QlItemPreview("", url);

		}

		public override nint PreviewItemCount(QLPreviewController controller)
		{
			return 1;
		}
    }
}
