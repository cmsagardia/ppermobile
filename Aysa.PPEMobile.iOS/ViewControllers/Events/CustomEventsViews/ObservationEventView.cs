using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using Aysa.PPEMobile.Model;
using CoreGraphics;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using System.IO;
using QuickLook;
using Aysa.PPEMobile.iOS.ViewControllers.Preview;
using Aysa.PPEMobile.iOS.Utilities;
using BigTed;

namespace Aysa.PPEMobile.iOS
{
    public partial class ObservationEventView : UIView, AttachmentFileViewDelegate
	{
        public ObservationEventView (IntPtr handle) : base (handle)
        {
        }

		public static ObservationEventView Create()
		{

			var arr = NSBundle.MainBundle.LoadNib("ObservationEventView", null, null);
			var v = Runtime.GetNSObject<ObservationEventView>(arr.ValueAt(0));

			return v;
		}

        public override void AwakeFromNib()
		{

		}

        public void LoadObservationInView(Observation observation)
        {
            var sectorName = observation.Sector == null ? "" : observation.Sector.Nombre;
            NameLabel.Text = $"{observation.Usuario.NombreApellido} - {sectorName}";
            DescriptionTextView.Text = observation.Observacion;
            DateLabel.Text = observation.Fecha.ToString(AysaConstants.FormatDate24Hrs);

			if (string.IsNullOrEmpty(observation.Observacion))
				DescriptionTextView.Hidden = true;

			if (observation.Archivos.Count > 0)
			{
				// Load AttachmentContentView with files of event
				LoadAttachmentsOfEvent(observation);
			}
			//else
			//{
			//	// Hidden conteiner
			//	AttachmentContentView.Hidden = true;
			//	//TopAttachmentContentConstraint.Constant = 0;
			//	//HeightAttachmentConstraint.Constant = 0;
			//	//View.LayoutIfNeeded();
			//}
		}

		public void AttachmentSelected(AttachmentFile documentFile)
		{
			//Show a HUD with a progress spinner and the text
			BTProgressHUD.Show("Descargando...", -1, ProgressHUD.MaskType.Black);
			DownloadFileToShowIt(documentFile);
		}

		private void DownloadFileToShowIt(AttachmentFile documentFile)
		{
			// Display an Activity Indicator in the status bar
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			Task.Run(async () =>
			{
				try
				{
					// Download file from server
					byte[] bytesArray = await AysaClient.Instance.GetFile(documentFile.Id);

					InvokeOnMainThread(() =>
					{
						BTProgressHUD.Dismiss();
						NSData data = NSData.FromArray(bytesArray);
						SaveFileInLocalFolder(data, documentFile);
					});

				}
				catch (HttpUnauthorized)
				{
					//InvokeOnMainThread(() =>
					//{
					//	ShowSessionExpiredError();
					//});
				}
				catch (Exception ex)
				{
					//InvokeOnMainThread(() =>
					//{
					//	ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					//});
				}
				finally
				{
					BTProgressHUD.Dismiss();
					// Dismiss an Activity Indicator in the status bar
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				}
			});
		}

		private void SaveFileInLocalFolder(NSData data, AttachmentFile documentFile)
		{
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var filename = Path.Combine(documents, documentFile.FileName);
			data.Save(filename, false);
			ShowFileSavedInPreviewController(documentFile);
		}

		private void ShowFileSavedInPreviewController(AttachmentFile fileAtt)
		{
			UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
			while (currentController.PresentedViewController != null)
				currentController = currentController.PresentedViewController;
			UIView currentView = currentController.View;

			QLPreviewController quickLookController = new QLPreviewController();
			quickLookController.DataSource = new PreviewControllerDataSource(fileAtt.FileName);

			currentController.PresentViewController(quickLookController, true, null);
		}

		public void RemoveAttachmentSelected(AttachmentFile documentFile)
        {
            throw new NotImplementedException();
        }

        private void LoadAttachmentsOfEvent(Observation observation)
		{
			// Remove all SubViews
			foreach (UIView attachmentView in AttachmentContentView.Subviews)
			{
				attachmentView.RemoveFromSuperview();
			}

			// Add attachment file in AttachmentContentView
			for (int i = 0; i < observation.Archivos.Count; i++)
			{
				// Create observation view
				AttachmentFileView attachmentView = AttachmentFileView.Create();

				// Get top position of attachment file
				int topPosition = i * 20;
				if(i == 0)
                {
					topPosition = i * 5;
                }
				attachmentView.Frame = new CGRect(0, topPosition, AttachmentContentView.Frame.Width, 30 - observation.Archivos.Count * 20 );
				attachmentView.Delegate = this;

				// Load file object in View
				attachmentView.LoadAttachmentFileInView(observation.Archivos[i], true);

				// Add attachment in Content View
				AttachmentContentView.AddSubview(attachmentView);
			}
		}
	}
}