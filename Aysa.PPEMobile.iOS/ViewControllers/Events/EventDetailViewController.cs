using System;
using UIKit;
using Foundation;
using Aysa.PPEMobile.Model;
using CoreGraphics;
using Aysa.PPEMobile.iOS.Utilities;
using BigTed;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using System.Collections.Generic;
using Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderPickerForTextField;
using System.IO;
using QuickLook;
using Aysa.PPEMobile.iOS.ViewControllers.Preview;
using System.Linq;
using MobileCoreServices;
using AssetsLibrary;
using System.Runtime.InteropServices;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events
{
    public partial class EventDetailViewController : UIViewController, AddEventViewControllerDelegate, PickerTextFieldDataSourceDelegate, AttachmentFileViewDelegate
    {
        // Public variables
        public Event EventDetail;
		bool privateFile = false;
		// Text Length Limits
		int obsLimit = 1000;

		// Private variables
		// Define general format for Dates
		private static readonly int HeightObservationView = 220;
        List<Section> ActiveSectionsList;
        Section SectionSelectedForObservation;

        private static readonly int HeightAttachmentView = 30;

		UIImagePickerController ImagePicker;
		UIDocumentPickerViewController DocumentPicker;
		List<AttachmentFile> AttachmentFilesInMemory = new List<AttachmentFile>();
		List<AttachmentFile> UploadedAttachmentFiles = new List<AttachmentFile>();
		List<AttachmentFile> filesToDeleteInServer = new List<AttachmentFile>();
		int CountAttachmentsUploaded = 0;


		public EventDetailViewController(IntPtr handle) : base(handle)
		{
		}


		public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			// Change status bar text color to white
			NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			CounterObservationText.Text = $"0/{obsLimit}";


			SetupStyleOfView();

			// Load IBOutlets with event properties
			LoadEventInView();
			ObservationTextView.ShouldChangeText += (UITextView textView, NSRange range, string text) =>
			{
				var newLength = textView.Text.Length + text.Length - range.Length;
				if(newLength <= obsLimit)
                {
					CounterObservationText.Text = $"{newLength}/{obsLimit}";
					return true;
                }
				ShowErrorAlert($"El campo Observacion no puede contener mas de {obsLimit} caracteres");
				return false;
			};

			// Get files of events
			GetFilesOfEventFromServer();

			// Load sections to select when user add an observation
			LoadActiveSectionsInView();		

			SetUpViewAccordingUserPermissions();
		}

		public override void ViewWillAppear(Boolean animated)
		{
			base.ViewWillAppear(animated);

		}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

		#region Internal Methods

		private void SetupStyleOfView()
		{
			UIColor borderColorContentView = UIColor.FromRGB(107, 107, 110);
			float borderWidth = 1.5f;
			float radius = 15.0f;

			// Add border and round of Tags
            /*ViewTagConteiner.Layer.BorderWidth = borderWidth;
			ViewTagConteiner.Layer.CornerRadius = radius;
			ViewTagConteiner.Layer.BorderColor = borderColorContentView.CGColor;*/

			// For some reason the event TouchUpInside don't response if it's assign from .Storyboard
            CheckButton.TouchUpInside += CheckAction;
			UploadFileButton.TouchUpInside += SelectFileAction;
			CreateObservationButton.TouchUpInside += CreateObservationAction;

			// Int ImagePickerViewController
			ImagePicker = new UIImagePickerController();
			ImagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
			ImagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
			ImagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
			ImagePicker.Canceled += Handle_Canceled;

			DocumentPicker = new UIDocumentPickerViewController(allowedUTIs, UIDocumentPickerMode.Import);
			DocumentPicker.AllowsMultipleSelection = true;
			DocumentPicker.WasCancelled += Picker_WasCancelled;
			DocumentPicker.DidPickDocumentAtUrls += Handle_DidPickDocumentAtUrls;
		}

		void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs media)
		{
			// determine what was selected, video or image
			bool isImage = false;
			switch (media.Info[UIImagePickerController.MediaType].ToString())
			{
				case "public.image":
					Console.WriteLine("Image selected");
					isImage = true;
					break;
				case "public.video":
					Console.WriteLine("Video selected");
					break;
			}

			BuildFileAttachment(media, isImage);

			ImagePicker.DismissViewController(true, null);
		}

		void Handle_Canceled(object sender, EventArgs e)
		{
			ImagePicker.DismissViewController(true, null);
		}

		private void BuildFileAttachment(UIImagePickerMediaPickedEventArgs media, bool isImage)
		{
			string name = "";

			// Build attachment and show in view

			NSUrl referenceURL = media.Info[new NSString("UIImagePickerControllerReferenceURL")] as NSUrl;
			if (referenceURL != null)
			{
				ALAssetsLibrary assetsLibrary = new ALAssetsLibrary();
				assetsLibrary.AssetForUrl(referenceURL, delegate (ALAsset asset) {

					ALAssetRepresentation representation = asset.DefaultRepresentation;

					if (representation != null)
					{
						name = representation.Filename;
					}

					string nameFile = name.Length > 0 ? name : "Archivo Adjunto " + (AttachmentFilesInMemory.Count + 1).ToString();

					AttachmentFile attachmentFile = new AttachmentFile();
					attachmentFile.FileName = nameFile;

					if (isImage)
					{
						// Get image and convert to BytesArray
						UIImage originalImage = media.Info[UIImagePickerController.OriginalImage] as UIImage;

						if (originalImage != null)
						{
							string extension = referenceURL.PathExtension;

							using (NSData imageData = originalImage.AsJPEG(0.5f))
							{
								Byte[] fileByteArray = new Byte[imageData.Length];
								if (fileByteArray.Length <= 10971520)
								{
									Marshal.Copy(imageData.Bytes, fileByteArray, 0, Convert.ToInt32(imageData.Length));
									attachmentFile.BytesArray = fileByteArray;

									AttachmentFilesInMemory.Add(attachmentFile);
								}
								else
									ShowErrorAlert("El archivo que intenta subir supera los 10 Mb permitidos");
							}
						}
					}
					else
					{
						NSUrl mediaURL = media.Info[UIImagePickerController.MediaURL] as NSUrl;

						if (mediaURL != null)
						{
							var videoData = NSData.FromUrl(mediaURL);
							Byte[] fileByteArray = new Byte[videoData.Length];

							if (fileByteArray.Length <= 10971520)
							{
								Marshal.Copy(videoData.Bytes, fileByteArray, 0, Convert.ToInt32(videoData.Length));
								attachmentFile.BytesArray = fileByteArray;
								AttachmentFilesInMemory.Add(attachmentFile);
							}
							else
								ShowErrorAlert("El archivo que intenta subir supera los 10 Mb permitidos");
						}
					}

					// Show file selected in view
					LoadAttachmentsOfObservation();

				}, delegate (NSError error) {
					return;
				});
			}
		}

		void Handle_DidPickDocumentAtUrls(object s, UIDocumentPickedAtUrlsEventArgs e)
		{
			//var success = false;
			string filename = e.Urls[0].LastPathComponent;

            if (!string.IsNullOrEmpty(filename))
            {
				filename = MakeSafeFileName(filename);
				filename = filename.Replace("_", "");
			}

			AttachmentFile attachmentFile = new AttachmentFile();
			// Some invaild file url returns null  
			NSData data = NSData.FromUrl(e.Urls[0]);
			if (data != null)
			{
				byte[] dataBytes = new byte[data.Length];

				if (dataBytes != null) 
                {
					if (dataBytes.Length <= 10971520)
					{
						System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
						string nameFile = filename.Length > 0 ? filename : "Archivo Adjunto " + (AttachmentFilesInMemory.Count + 1).ToString();
						attachmentFile.FileName = nameFile;
						attachmentFile.Private = privateFile;
						attachmentFile.BytesArray = dataBytes;

						AttachmentFilesInMemory.Add(attachmentFile);
						//success = true;
					}
					else
					{
						ShowErrorAlert("El archivo que intenta subir supera los 10 Mb permitidos");
					}
				}
				else
				{
					ShowErrorAlert("No se pudo cargar el archivo. Intentelo nuevamente");
				}
			}
			LoadAttachmentsOfObservation();
		}

		private static readonly Dictionary<char, char> AndroidAllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ._-+,@£$€!½§~'=()[]{}0123456789 ".ToDictionary(c => c);
		private string MakeSafeFileName(string fileName)
		{
			char[] newFileName = fileName.ToCharArray();
			for (int i = 0; i < newFileName.Length; i++)
			{
				if (!AndroidAllowedChars.ContainsKey(newFileName[i]))
					newFileName[i] = '_';
			}
			return new string(newFileName);
		}

		private void Picker_WasCancelled(object sender, EventArgs e)
		{
			DocumentPicker.DismissViewController(true, null);
		}

		private string[] allowedUTIs =
		{
			UTType.Image,
			UTType.PNG,
			UTType.JPEG,
			UTType.RTF,
			UTType.TIFF,
			UTType.BMP,
			UTType.RawImage,
			UTType.PlainText,
			UTType.UTF8PlainText,
			UTType.UTF16PlainText,
			UTType.Text,
			UTType.PDF,
			UTType.ZipArchive,
			UTType.Spreadsheet,
			UTType.MP3,
			UTType.Video,
			UTType.MPEG2Video,
			UTType.Movie,
			UTType.AppleProtectedMPEG4Video,
			UTType.AVIMovie,
			UTType.MPEG4,
			UTType.MPEG4Audio,
			UTType.WaveformAudio,
			UTType.Audio,
			"com.microsoft.word.doc",
			"com.microsoft.excel.xls",
			"com.microsoft.powerpoint.ppt",
			"com.microsoft.waveform-​audio",
			"com.microsoft.windows-​media-wmv",
			"org.openxmlformats.wordprocessingml.document",
			"org.openxmlformats.wordprocessingml.document",
			"org.openxmlformats.spreadsheetml.sheet",
			"org.openxmlformats.presentationml.presentation"
		};

		public void SelectFileAction(object sender, EventArgs e)
		{
			// Show picker to select file
			UIAlertController alert = UIAlertController.Create(
				"Aviso",
				"Seleccione Ubicación de Adjunto",
				UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("Galeria", UIAlertActionStyle.Default, action =>
			{
				this.PresentViewController(ImagePicker, true, null);
			}));

			alert.AddAction(UIAlertAction.Create("Almacenamiento", UIAlertActionStyle.Default, action =>
			{
				this.PresentViewController(DocumentPicker, true, null);
			}));

			PresentViewController(alert, animated: true, completionHandler: null);
			//this.NavigationController.PresentViewController(ImagePicker, true, null);
		}

		private void LoadEventInView()
        {
			EditButton.Enabled = EventDetail.CanEdit;

			TitleEventLabel.Text = EventDetail.Titulo;
            DateLabel.Text = EventDetail.Fecha.ToString(AysaConstants.FormatDate24Hrs);
            PlaceLabel.Text = EventDetail.Lugar;
            TypeLabel.Text = EventDetail.Tipo.Nombre;

			// Status 1 = Open
			// Status 2 = Close
			StatusLabel.Text = EventDetail.Estado == 1 ? "Abierto" : "Cerrado";

			ViewTagConteiner.Hidden = false;

			var roles = UserSession.Instance.Roles;

			if (!roles.Any(r => r.Nombre.Equals("Directorio")))
			{
				CheckPrivate.Hidden = true;
				PrivateLabel.Hidden = true;
			}

			// Set deail event and setup format for DetailLabel
			LoadDetailLabel();

			LoadSectionLabel();

			LoadUserLabel();


			if (EventDetail.Observaciones != null)
            {
				// Load list of Observations
				LoadObservationsOfEvent();
            }

			// Define confidential 
			string nameImage = EventDetail.Confidencial ? "checked" : "unchecked";
			CheckButton.SetImage(UIImage.FromBundle(nameImage), UIControlState.Normal);
		}

		private void HiddenSectionForAddObservation()
        {
            SectionConteinerView.Hidden = true;
            HeightAddObservationConstraint.Constant = HeightAddObservationConstraint.Constant - HeightSectionConteinerViewConstraint.Constant;
            HeightSectionConteinerViewConstraint.Constant = 0;
            TopSectionConteinerViewConstraint.Constant = 0;
            View.LayoutIfNeeded();
        }

        private void SetUpViewAccordingUserPermissions()
        {

            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.ModificarEvento))
			{
                // Allow to user do everything

                // Check if user doesn't have section active, in this case hide section combo, and set section assigned of event
                if(!UserSession.Instance.CheckIfUserHasActiveSections())
                {
                    SectionSelectedForObservation = EventDetail.Sector;
                    //HiddenSectionForAddObservation();
                }

				// Check if user can edit confidential field.
				// The user can edit confidential field if he has guard responsable in section 1 or 2
				if (!UserSession.Instance.CheckIfUserIsGuardResponsableOfMainSection())
				{
					// The user doesn't have guard responsable in section 1 or 2, he can't edit confidential field
					//HiddenConfidentialField();
				}

				return;
            }else{
                if (UserSession.Instance.CheckIfUserHasPermission(Permissions.ModificarEventoAutorizado))
                {
                    if(UserSession.Instance.CheckIfUserHasActiveSections())
                    {
                        // User has active section so he can add observations

                        // Check if user can edit confidential field.
                        // The user can edit confidential field if he has guard responsable in section 1 or 2
                        if(!UserSession.Instance.CheckIfUserIsGuardResponsableOfMainSection())
                        {
                            // The user doesn't have guard responsable in section 1 or 2, he can't edit confidential field
							//HiddenConfidentialField();
                        }

                        return;
                    }else{
                        // User doesn't have active sections so he can't add observations
                        // The user only can edit the events that they were created by himself
                       // HiddenAddObservationSection();
                        //HiddenConfidentialField();
                        return;
                    }
                }
            }

			//// User doesn't have any permissions
			//// Disable edit button, add observations and confidential field
			//HiddenAddObservationSection();
            //HiddenConfidentialField();
        }

        private void HiddenAddObservationSection()
        {
			
			AddObservationView.Hidden = true;
			HeightAddObservationConstraint.Constant = 0;
			View.LayoutIfNeeded();
        }

        private void HiddenConfidentialField()
        {
			ConfidentialConteinerView.Hidden = true;
			HeightConfidentialConteinerConstraint.Constant = 0;
			TopConfidentialContainerConstraint.Constant = 0;
			View.LayoutIfNeeded();
        }

        private void LoadDetailLabel()
        {
			// Set "Detalle" of Detail label text bold and another part default
			string detailText = "Detalle: " + EventDetail.Detalle;
            var strText = new NSMutableAttributedString(detailText);
            strText.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("Helvetica Bold", 15), new NSRange(0, 8));
            strText.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.FromRGB(77, 77, 81), new NSRange(0, 8));

            DetailLabel.AttributedText = strText;
        }

		private void LoadSectionLabel()
		{
			// Set "Sector" of Section label text bold and another part default

			string detailText = "Sector: " + EventDetail.Sector.Nombre;
			var strText = new NSMutableAttributedString(detailText);
			strText.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("Helvetica Bold", 15), new NSRange(0, 7));
			strText.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.FromRGB(77, 77, 81), new NSRange(0, 7));

			SectionLabel.AttributedText = strText;
		}

		private void LoadUserLabel()
        {
			// Set "UserName" of tag label text bold and another part default

			string detailText = "Usuario Creador: " + EventDetail.Usuario.NombreApellido;
			var strText = new NSMutableAttributedString(detailText);
			strText.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("Helvetica Bold", 15), new NSRange(0, 17));
			strText.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.FromRGB(77, 77, 81), new NSRange(0, 17));

			TagLabel.AttributedText = strText;
		}


		private void LoadObservationsOfEvent()
        {
            // Clear conteiner of Observations views

            // Remove all SubViews
            foreach(UIView observationView in ObservationContentView.Subviews)
            {
                observationView.RemoveFromSuperview();
            }

            // Set height of ObservationContentView
			AdjustSizeObservationsContentView();

			var roles = UserSession.Instance.Roles;

			if (!roles.Any(r => r.Nombre.Equals("Directorio")))
			{
				EventDetail.Observaciones = EventDetail.Observaciones.Where(x => !x.Directorio).ToList();
			}


			EventDetail.Observaciones = EventDetail.Observaciones.OrderByDescending(x => x.Fecha).ToList();

			int countArchivos = 0;
			// Add observations in ObservationContentView
			for (int i = 0; i < EventDetail.Observaciones.Count; i++)
            {
				// Create observation view
				ObservationEventView observationView = ObservationEventView.Create();

				// Get top position of observation
				int height = EventDetail.Observaciones[i].Archivos.Count() * 20;
                int topPosition = i * HeightObservationView + countArchivos;
                observationView.Frame = new CGRect(0, topPosition, ObservationContentView.Frame.Width, height + HeightObservationView);

                // Load observation object in View
                observationView.LoadObservationInView(EventDetail.Observaciones[i]);

                // Add Observation in Content View
				ObservationContentView.AddSubview(observationView);
				countArchivos += height + 2;
            }
		}

		private void AdjustSizeObservationsContentView()
		{
			// Set Height of Observation ContentView according to count of observations in Event
			ConstraintHeightObservationContentView.Constant = EventDetail.Observaciones.Count * 300;
			View.LayoutIfNeeded();
		}

		private void LoadAttachmentsOfEvent()
		{

			// Remove all SubViews
			foreach (UIView attachmentView in AttachmentContentView.Subviews)
			{
				attachmentView.RemoveFromSuperview();
			}

			// Set height of AttachmentContentView
			AdjustSizeAttachmentContentView();

			// Add attachment file in AttachmentContentView
            for (int i = 0; i < EventDetail.Archivos.Count; i++)
			{
				// Create observation view
                AttachmentFileView attachmentView = AttachmentFileView.Create();

				// Get top position of attachment file
				int topPosition = i * HeightAttachmentView;
                attachmentView.Frame = new CGRect(0, topPosition, AttachmentContentView.Frame.Width, HeightAttachmentView);
                attachmentView.Delegate = this;

				// Load file object in View
                attachmentView.LoadAttachmentFileInView(EventDetail.Archivos[i], true);

				// Add attachment in Content View
                AttachmentContentView.AddSubview(attachmentView);
			}
		}

		private void LoadAttachmentsOfObservation()
		{
			// Remove all SubViews
			foreach (UIView attachmentView in AttachmentObsContentView.Subviews)
			{
				attachmentView.RemoveFromSuperview();
			}

			// Set height of AttachmentObsContentView
			AdjustSizeAttachmentContentView();

			// Add attachment file in AttachmentObsContentView
			for (int i = 0; i < AttachmentFilesInMemory.Count; i++)
			{
				// Create observation view
				AttachmentFileView attachmentView = AttachmentFileView.Create();

				// Get top position of attachment file
				int topPosition = i * HeightAttachmentView;
				attachmentView.Frame = new CGRect(0, topPosition, AttachmentObsContentView.Frame.Width, HeightAttachmentView);
				attachmentView.Delegate = this;

				// Load file object in View
				attachmentView.LoadAttachmentFileInView(AttachmentFilesInMemory[i], false);

				// Add attachment in Content View
				AttachmentObsContentView.AddSubview(attachmentView);
			}
		}

		private void AdjustSizeAttachmentContentView()
		{
			// Set Height of AttachmentContentView according to count of files in Event
			AttachmentContentView.Hidden = false;
			HeightAttachmentConstraint.Constant = EventDetail.Archivos.Count * HeightAttachmentView;
			View.LayoutIfNeeded();
		}

		private void ShowErrorAlert(string message)
		{
			var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
			PresentViewController(alert, true, null);
		}

		private void ShowSessionExpiredError()
		{
			UIAlertController alert = UIAlertController.Create("Aviso", "Su sesión ha expirado, por favor ingrese sus credenciales nuevamente", UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, action => {
				// Send notification to TabBarController to return to login
				NSNotificationCenter.DefaultCenter.PostNotificationName("SessionExpired", this);
			}));

			PresentViewController(alert, animated: true, completionHandler: null);
		}

		public void LoadActiveSectionsInView()
		{
			ActiveSectionsList = UserSession.Instance.ActiveSections;

			var roles = UserSession.Instance.Roles;
			var personGuard = UserSession.Instance.PersonInGuard;
			var sections = personGuard.Sectores;

			// After get Sections Active list, load elements in PickerView
			LoadPickerViewInSectionTextField();

            // Load by default the first section
            if(ActiveSectionsList.Count > 0)
            {
				SectionSelectedForObservation = ActiveSectionsList[0];
				SectionTextField.Text = string.Format("{0}", SectionSelectedForObservation.Nombre);
				//SectionTextField.Text = string.Format("{0} - Nivel: {1}", SectionSelectedForObservation.Nombre, SectionSelectedForObservation.Nivel);
			}

			if (!EventDetail.CanEdit)
			{
				HiddenAddObservationSection();
			}


			if (roles.Any(r => r.Nombre.Equals("Referente PPE")) || sections.Any(x => x.Nivel == 1))
				ConfidentialConteinerView.Hidden = false;
			else
				ConfidentialConteinerView.Hidden = true;
		}

		private void LoadPickerViewInSectionTextField()
		{

			// Check Active Sections have values
			if (ActiveSectionsList == null)
			{
				return;
			}

			// Build data from list of EventsType
			List<string> data = new List<string>();

			for (int i = 0; i < ActiveSectionsList.Count; i++)
			{
				Section sectionObj = ActiveSectionsList[i];
				data.Add(string.Format("{0}", sectionObj.Nombre));
				//data.Add(string.Format("{0} - Nivel: {1}", sectionObj.Nombre, sectionObj.Nivel));
			}

			// Load Picker for TypeTextField
			UIPickerView picker = new UIPickerView();
            PickerTextFieldDataSource modelPicker = new PickerTextFieldDataSource(data, SectionTextField);
			modelPicker.Delegate = this;
			picker.Model = modelPicker;

			SectionTextField.InputView = picker;

		}

		private User GetUserLogged()
		{
			User user = new User();
			user.UserName = UserSession.Instance.UserName;
			user.Id = UserSession.Instance.Id;
			user.NombreApellido = UserSession.Instance.nomApel;
			return user;
		}

		private Event GetEventAssociated()
		{
			// Build event
			Event eventObservation = new Event();
			eventObservation.Id = EventDetail.Id;

			return eventObservation;
		}

		private void UploadFilesToServer()
		{
			// Upload files
			Task.Run(async () =>
			{
				try
				{
					AttachmentFile fileInMemory = AttachmentFilesInMemory[CountAttachmentsUploaded];

					// If Attachment file doesn't have file that means that it's already added
					if (fileInMemory.BytesArray == null)
					{
						CountAttachmentsUploaded++;
						if (CountAttachmentsUploaded < AttachmentFilesInMemory.Count)
						{
							UploadFilesToServer();
						}
						else
						{
							InvokeOnMainThread(() =>
							{
								Observation observationObj = BuildObservation();
								if (observationObj != null)
									SendObservationToServer(observationObj);
							});
						}

						return;
					}

					AttachmentFile fileUpload = await AysaClient.Instance.UploadFile(fileInMemory.BytesArray, fileInMemory.FileName);
	
					InvokeOnMainThread(() =>
					{
						fileUpload.Private = privateFile;
						// Save uploaded file in list, this list will be assigned to the event that it will be created
						UploadedAttachmentFiles.Add(fileUpload);
						CountAttachmentsUploaded++;

						if (CountAttachmentsUploaded < AttachmentFilesInMemory.Count)
						{
							UploadFilesToServer();
						}
						else
						{
							Observation observationObj = BuildObservation();
							if (observationObj != null)
								SendObservationToServer(observationObj);
						}

					});
				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
						ShowSessionExpiredError();
						// Remove progress
						BTProgressHUD.Dismiss();
					});
				}
				catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
						// Remove progress
						BTProgressHUD.Dismiss();
						CountAttachmentsUploaded = 0;
					});
				}
			});
		}


		private Observation BuildObservation()
		{
		    Observation observationObj = new Observation();
            observationObj.Fecha = DateTime.Now;
            observationObj.Observacion = ObservationTextView.Text;
            observationObj.Evento = GetEventAssociated();
            observationObj.Usuario = GetUserLogged();
            observationObj.Sector = SectionSelectedForObservation;
			observationObj.Directorio = privateFile;
			observationObj.Archivos = UploadedAttachmentFiles;
			return observationObj;
        }

		private void SendObservationToServer(Observation observationObj)
		{
			Task.Run(async () =>
			{

				try
				{
					Observation observation = await AysaClient.Instance.CreateObservation(observationObj);

					InvokeOnMainThread(() =>
					{
						if (observation != null)
						{
							if (EventDetail.Observaciones != null)
							{
                                // Clear TextField
                                ObservationTextView.Text = "";
								// Add observetion in fist position to show it in first place
								EventDetail.Observaciones.Insert(0, observation);
								// Clear file label
								DeleteFile(observationObj.Archivos.FirstOrDefault());
								// Initial counter 
								CountAttachmentsUploaded = 0;
								// Reset ListFile
								AttachmentFilesInMemory = new List<AttachmentFile>();
								UploadedAttachmentFiles = new List<AttachmentFile>();
								// Reload list of Observations
								LoadObservationsOfEvent();
							}
						}

                        BTProgressHUD.ShowImage(UIImage.FromBundle("ok_icon"), "La observación ha sido creada con éxito", 2000);

					});

				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
						// Remove progress
						BTProgressHUD.Dismiss();

						ShowSessionExpiredError();
					});
				}
				catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
						// Remove progress
						BTProgressHUD.Dismiss();

						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
				}
			});
		}

		private void ChangeEventStatusCenfidential()
		{
			//Show a HUD with a progress spinner and the text
			BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

			Task.Run(async () =>
			{

				try
				{
                    await AysaClient.Instance.SetEventConfidential(EventDetail.Id);

					InvokeOnMainThread(() =>
					{

						// Set inverse image
						string nameImage = EventDetail.Confidencial ? "unchecked" : "checked";

						CheckButton.SetImage(UIImage.FromBundle(nameImage), UIControlState.Normal);

                        // Set inverse 
                        EventDetail.Confidencial = !EventDetail.Confidencial;

						// Remove progress
						BTProgressHUD.Dismiss();
					});

				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
						// Remove progress
						BTProgressHUD.Dismiss();

						ShowSessionExpiredError();
					});
				}
				catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
						// Remove progress
						BTProgressHUD.Dismiss();

						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
				}
			});
		}

        private void GetFilesOfEventFromServer()
		{
            
			// Display an Activity Indicator in the status bar
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			Task.Run(async () =>
			{

				try
				{
                    // Get Files
                    EventDetail.Archivos = await AysaClient.Instance.GetFilesOfEvent(EventDetail.Id);

					InvokeOnMainThread(() =>
					{
                        if (EventDetail.Archivos.Count > 0)
                        {
							// Load AttachmentContentView with files of event
							LoadAttachmentsOfEvent();
                        }
                        else
                        {
                            // Hidden conteiner
                            AttachmentContentView.Hidden = true;
                            TopAttachmentContentConstraint.Constant = 0;
							HeightAttachmentConstraint.Constant = 0;
							View.LayoutIfNeeded();
                        }

					});

				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
						ShowSessionExpiredError();
					});
				}
				catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
				}
				finally
				{
					// Dismiss an Activity Indicator in the status bar
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				}
			});

		}

		private void DownloadFileToShowIt(AttachmentFile documentFile)
		{
			// Display an Activity Indicator in the status bar
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			Task.Run(async () =>
			{

				try
				{
					if (documentFile.Id != null)
					{
						// Download file from server
						byte[] bytesArray = await AysaClient.Instance.GetFile(documentFile.Id);
						//var text = System.Text.Encoding.Default.GetString(bytesArray);
						//text = text.Replace("\"", "");
						//bytesArray = Convert.FromBase64String(text);

						InvokeOnMainThread(() =>
						{
							BTProgressHUD.Dismiss();
							NSData data = NSData.FromArray(bytesArray);
							SaveFileInLocalFolder(data, documentFile);
						});
					}
					else
					{
						int index = -1;

						for (int i = 0; i < AttachmentFilesInMemory.Count; i++)
						{
							AttachmentFile file = AttachmentFilesInMemory[i];
							if (file.FileName == documentFile.FileName)
							{
								index = i;
								// Break the loop
								i = AttachmentFilesInMemory.Count;
							}
						}

						if (index != -1)
						{
							BTProgressHUD.Dismiss();
							AttachmentFile fileInMemory = AttachmentFilesInMemory[index];
							InvokeOnMainThread(() =>
							{
								NSData data = NSData.FromArray(fileInMemory.BytesArray);
								SaveFileInLocalFolder(data, fileInMemory);
							});
						}
					}

				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
						BTProgressHUD.Dismiss();
						ShowSessionExpiredError();
					});
				}
				catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
						BTProgressHUD.Dismiss();
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
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

			QLPreviewController quickLookController = new QLPreviewController();
            quickLookController.DataSource = new PreviewControllerDataSource(fileAtt.FileName);
			NavigationController.PushViewController(quickLookController, true);

		}

		#endregion


		#region Implement Delegates of AddEventViewControllerDelegate 

		// This delegate will be called when the event has been updated
		public void EventWasUpdated(Event eventObj)
        {
            // Load event updated in View
            EventDetail = eventObj;
            LoadEventInView();

			// Get files of events
			GetFilesOfEventFromServer();

        }

		#endregion

		#region Implement Delegates of AttachmentFileViewDelegate 

        public void AttachmentSelected(AttachmentFile documentFile)
		{  
			//Show a HUD with a progress spinner and the text
			BTProgressHUD.Show("Descargando...", -1, ProgressHUD.MaskType.Black);
			DownloadFileToShowIt(documentFile);
        }

        public void RemoveAttachmentSelected(AttachmentFile documentFile)
		{
			UIAlertController alert = UIAlertController.Create("Aviso", "¿Está seguro que desea quitar el Archivo?", UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, null));
			alert.AddAction(UIAlertAction.Create("Si", UIAlertActionStyle.Default, action => {
				// Delete file
				DeleteFile(documentFile);
			}));

			PresentViewController(alert, animated: true, completionHandler: null);
		}

		private void DeleteFile(AttachmentFile documentFile)
		{
			RemoveFileFromView(documentFile);
		}

		private void RemoveFileFromView(AttachmentFile documentFile)
		{
			// Search file to remove
			int index = -1;

			for (int i = 0; i < AttachmentFilesInMemory.Count; i++)
			{
				AttachmentFile file = AttachmentFilesInMemory[i];
				if (file.FileName == documentFile.FileName)
				{
					index = i;
					// Break the loop
					i = AttachmentFilesInMemory.Count;
				}
			}

			if (index != -1)
			{
				AttachmentFilesInMemory.RemoveAt(index);
				// Reload files attachments
				LoadAttachmentsOfObservation();
			}
		}

		#endregion

		#region IBActions

		public void CheckAction(object sender, EventArgs e)
		{
            
            string message = EventDetail.Confidencial ? "¿Está seguro que desea desmarcar el evento como confidencial?" : "¿Está seguro que desea marcar el evento como confidencial?";

            UIAlertController alert = UIAlertController.Create("Aviso", message, UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, null));

			alert.AddAction(UIAlertAction.Create("Si", UIAlertActionStyle.Default, action => {

                ChangeEventStatusCenfidential();
			}));

			PresentViewController(alert, animated: true, completionHandler: null);
		}

		public void CreateObservationAction(object sender, EventArgs e)
		{
            if (string.IsNullOrEmpty(ObservationTextView.Text) && AttachmentFilesInMemory.Count == 0)
            {
				// There are errors in the Input fields.. show Alert
				ShowErrorAlert($"La observación debe tener un comentario o un adjunto cargado!");
			}
            else
            {
                if (IsMaxCharactersTextView(ObservationTextView))
                {
					ShowErrorAlert($"El campo Observacion no puede contener mas de {obsLimit} caracteres");
					BTProgressHUD.Dismiss();
					return;
                }
				//Show a HUD with a progress spinner and the text
				BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

				if (AttachmentFilesInMemory.Count > 0)
				{
					UploadFilesToServer();
				}
				else
				{
					Observation observationObj = BuildObservation();
					if (observationObj != null)
						SendObservationToServer(observationObj);
				}
			}
		}

		private bool IsMaxCharactersTextView(UITextView textView)
		{
			if (textView.Text.Length > obsLimit)
			{

				textView.Layer.BorderColor = UIColor.Red.CGColor;
				textView.Layer.BorderWidth = 1.0f;

				return true;
			}
			else
			{
				textView.Layer.BorderColor = UIColor.Clear.CGColor;
				textView.Layer.BorderWidth = 1.0f;

				return false;
			}
		}

		#endregion

		#region Implement PickerTextFieldDataSourceDelegate Metods

		public void ItemSelectedValue(int indexSelected, UITextField textField)
		{

            SectionSelectedForObservation = ActiveSectionsList[indexSelected];
			SectionTextField.Text = string.Format("{0}", SectionSelectedForObservation.Nombre);
			//SectionTextField.Text = string.Format("{0} - Nivel: {1}", SectionSelectedForObservation.Nombre, SectionSelectedForObservation.Nivel);
		}

		#endregion

		#region Navigation Methods

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "EditEventSegue")
			{
                AddEventViewController addEventViewController = (AddEventViewController)segue.DestinationViewController;
                addEventViewController.EditEvent = EventDetail;
                addEventViewController.Delegate = this;	
            }

		}

		#endregion

		partial void checkButton_TouchUpInside(UIButton sender)
		{

			sender.Selected = !sender.Selected;
			string nameImage = CheckPrivate.Selected ? "checked" : "unchecked";

			CheckPrivate.SetImage(UIImage.FromBundle(nameImage), UIControlState.Normal);

			if (CheckPrivate.Selected)
				privateFile = true;
			else
				privateFile = false;
		}
	}
}

