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
using Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderPickerForTextField;
using System.IO;
using QuickLook;
using Aysa.PPEMobile.iOS.ViewControllers.Preview;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
    public partial class FeatureDetailViewController : UIViewController, AddFeatureViewControllerDelegate, PickerTextFieldDataSourceDelegate, AttachmentFileViewDelegate
    {
        // Public variables
        public Feature FeatureDetail;

        // Private variables
        // Define general format for Dates
        private static readonly int HeightObservationView = 140;
        List<Section> ActiveSectionsList;
        Section SectionSelectedForObservation;

        private static readonly int HeightAttachmentView = 30;


        public FeatureDetailViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;

            // Load IBOutlets with event properties
            LoadFeatureInView();

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



        private void LoadFeatureInView()
        {
            // Format NSDate
            NSDateFormatter dateFormat = new NSDateFormatter();
            dateFormat.DateFormat = "yyyy-MM-dd HH:mm";

            DateLabel.Text = FeatureDetail.Date.ToString(AysaConstants.FormatDate24Hrs);
            TitleEventLabel.Text = FeatureDetail.Detail;
            AutorLabel.Text = FeatureDetail.Usuario.NombreApellido;
            SectionTextField.Text = FeatureDetail.Sector.Nombre;


            // Hide detail label by duplication
            DetailLabel.Hidden = true;

            // Set deail event and setup format for DetailLabel
            //LoadDetailLabel();
        }


        private void SetUpViewAccordingUserPermissions()
        {

            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.ModificarEvento))
            {
                // Allow to user do everything

                // Check if user doesn't have section active, in this case hide section combo, and set section assigned of event
                if (!UserSession.Instance.CheckIfUserHasActiveSections())
                {

                }

                // Check if user can edit confidential field.
                // The user can edit confidential field if he has guard responsable in section 1 or 2
                if (!UserSession.Instance.CheckIfUserIsGuardResponsableOfMainSection())
                {
                    // The user doesn't have guard responsable in section 1 or 2, he can't edit confidential field

                }

                return;
            }
            

            //// User doesn't have any permissions
            //// Disable edit button, add observations and confidential field
            EditButton.Enabled = FeatureDetail.CanEdit;
        }


        private void LoadDetailLabel()
        {
            // Set "Detalle" of Detail label text bold and another part default
            string detailText = "Detalle: " + FeatureDetail.Detail;
            var strText = new NSMutableAttributedString(detailText);
            strText.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("Helvetica Bold", 15), new NSRange(0, 8));
            strText.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.FromRGB(77, 77, 81), new NSRange(0, 8));

            DetailLabel.AttributedText = strText;
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
            for (int i = 0; i < FeatureDetail.Archivos.Count; i++)
            {
                // Create observation view
                AttachmentFileView attachmentView = AttachmentFileView.Create();

                // Get top position of attachment file
                int topPosition = i * HeightAttachmentView;
                attachmentView.Frame = new CGRect(0, topPosition, AttachmentContentView.Frame.Width, HeightAttachmentView);
                attachmentView.Delegate = this;

                // Load file object in View
                attachmentView.LoadAttachmentFileInView(FeatureDetail.Archivos[i], true);

                // Add attachment in Content View
                AttachmentContentView.AddSubview(attachmentView);
            }
        }

        private void AdjustSizeAttachmentContentView()
        {
            // Set Height of AttachmentContentView according to count of files in Event
            AttachmentContentView.Hidden = false;
            HeightAttachmentConstraint.Constant = FeatureDetail.Archivos.Count * HeightAttachmentView;
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
                PerformSegue("expiredSession", null);
            }));

            PresentViewController(alert, animated: true, completionHandler: null);
        }

        public void LoadActiveSectionsInView()
        {

            ActiveSectionsList = UserSession.Instance.ActiveSections;
            // After get Sections Active list, load elements in PickerView
            LoadPickerViewInSectionTextField();

            // Load by default the first section
            if (ActiveSectionsList.Count > 0)
            {
                SectionSelectedForObservation = ActiveSectionsList[0];
                SectionTextField.Text = string.Format("{0}", FeatureDetail.Sector.Nombre);
                //SectionTextField.Text = string.Format("{0} - Nivel: {1}", FeatureDetail.Sector.Nombre, FeatureDetail.Sector.Nivel);
            }


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

            //SectionTextField.InputView = picker;
            SectionTextField.UserInteractionEnabled = false;

        }

        private User GetUserLogged()
        {
            User user = new User()
            {
                Id = UserSession.Instance.Id,
                UserName = UserSession.Instance.UserName,
                NombreApellido = UserSession.Instance.nomApel
            };

            return user;
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
                    FeatureDetail.Archivos = await AysaClient.Instance.GetFilesOfFeature(FeatureDetail.Id);

                    InvokeOnMainThread(() =>
                    {
                        if (FeatureDetail.Archivos.Count > 0)
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
                        //ShowErrorAlert("Error al obtener archivos del servidor.");
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
                        //ShowErrorAlert("Error al descargar los archivos.");
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


        #region Implement Delegates of AddFeatureViewControllerDelegate 

        // This delegate will be called when the event has been updated
        public void FeatureWasUpdated(Feature eventObj)
        {
            // Load event updated in View
            FeatureDetail = eventObj;
            LoadFeatureInView();

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

        public void RemoveAttachmentSelected(AttachmentFile documentFile) { }

        #endregion

        #region IBActions


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
            if (segue.Identifier == "EditFeatureSegue")
            {
                AddFeatureViewController addFeatureViewController = (AddFeatureViewController)segue.DestinationViewController;
                addFeatureViewController.EditFeature = FeatureDetail;
                addFeatureViewController.Delegate = this;
            }
            if (segue.Identifier == "expiredSession")
            {
                LoginViewController loginViewController = (LoginViewController)segue.DestinationViewController;
            }

        }



        #endregion
    }
}

