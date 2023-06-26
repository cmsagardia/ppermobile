using System;
using UIKit;
using CoreGraphics;
using Foundation;
using Aysa.PPEMobile.iOS.Utilities;
using Aysa.PPEMobile.Model;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using BigTed;
using System.Collections.Generic;
using Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderPickerForTextField;
using ObjCRuntime;
using System.Runtime.InteropServices;
using AssetsLibrary;
using System.IO;
using MobileCoreServices;
using System.Text.RegularExpressions;
using System.Linq;
using QuickLook;
using Aysa.PPEMobile.iOS.ViewControllers.Preview;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
    /// <summary>
    /// / Deffine Interface to notify that an event was updated
    /// </summary>
    public interface AddFeatureViewControllerDelegate
    {
        void FeatureWasUpdated(Feature eventObj);
    }

    public partial class AddFeatureViewController : UIViewController, PickerTextFieldDataSourceDelegate, IUINavigationControllerDelegate, AttachmentFileViewDelegate
    {
        // Private IBOutlets
        private UIDatePicker PickerFromDate;
        private Semana semana;
        private List<Roles> roles = UserSession.Instance.Roles;
        private string oldSectionSelected;
        private string newSectionSelected;
        private bool FirstLoadDate = true;

        // Private Variables
        List<Section> ActiveSectionsList;
        Section SectionSelected;

        // Flag to avoid load Attachment events many times
        bool FilesAttachmentsAreLoaded = false;

        // Text Length Limits
        int detailLimit = 1000;

        UIImagePickerController ImagePicker;
        UIDocumentPickerViewController DocumentPicker;
        List<AttachmentFile> AttachmentFilesInMemory = new List<AttachmentFile>();
        List<AttachmentFile> UploadedAttachmentFiles = new List<AttachmentFile>();
        List<AttachmentFile> filesToDeleteInServer = new List<AttachmentFile>();
        int CountAttachmentsUploaded = 0;
        private static readonly int HeightAttachmentView = 30;

        // Public variables
        public Feature EditFeature;

        public Feature objectGlobal;

        /// <summary>
        ///  Define Delegate
        /// </summary>
        WeakReference<AddFeatureViewControllerDelegate> _delegate;

        public AddFeatureViewControllerDelegate Delegate
        {
            get
            {
                AddFeatureViewControllerDelegate workerDelegate;
                return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
            }
            set
            {
                _delegate = new WeakReference<AddFeatureViewControllerDelegate>(value);
            }
        }

        #region UIViewController Lifecycle

        public AddFeatureViewController(IntPtr handle) : base(handle)
        {

        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            GetUserInfo();
            GetActiveWeek();


            LoadActiveSectionsInView();
            await SetUpInputIBOutlets();
            CounterDetailText.Text = $"0/{detailLimit}";

            if (EditFeature != null)
            {
                // The user is editing an event
                PrepareViewToEditEvent();
            }

            TitleTextView.ShouldChangeText += (UITextView textView, NSRange range, string text) =>
            {
                var newLength = textView.Text.Length + text.Length - range.Length;
                IsMaxCharactersTextView(textView, (detailLimit - 5));
                if(newLength <= detailLimit)
                {
                    CounterDetailText.Text = $"{newLength}/{detailLimit}";
                    return true;
                }
                ShowErrorAlert($"Detalle: \nAlcanzó el límite de {detailLimit} caracterres");
                return false;
            };

            //TitleTextView.ShouldChangeText += (UITextView textView, NSRange range, string text) =>
            //{
            //    var isMatch = Regex.IsMatch(text, @"^[\¬\~\`\^\{\}\[\]\°\!\?\|\%\/\\\&\?\*\'\!]+$");
            //    if (isMatch)
            //    {
            //        return false;
            //    }

            //    return true;
            //};
        }

        private async void GetActiveWeek()
        {
            semana = await AysaClient.Instance.GetActiveWeek();
        }

        private async Task UpdateDateBySection(string id)
        {
            if (SectionSelected != null || id!= null)
            {
                string idSection = id != null ? id : SectionSelected.Id;
                newSectionSelected = idSection;
                //If need update Date?
                if (oldSectionSelected != newSectionSelected)
                {
                    await GetDateBySection(idSection);
                    oldSectionSelected = newSectionSelected;
                }
            }
        }

        private async Task GetDateBySection(string idSection)
            {
            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            try
            {

                SemanaSeccion date = await AysaClient.Instance.SearchDateBySection(idSection);
                semana.Desde = date.Desde;
                semana.Hasta = date.Hasta;

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
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Load files after calling ViewDidLayoutSubviews
            if (!FilesAttachmentsAreLoaded)
            {

                // Load file attachments after that the view was loaded
                if (EditFeature != null)
                {

                    LoadAttachmentsOfEvent();
                }

                FilesAttachmentsAreLoaded = true;
            }

        }



        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #endregion

        #region Private Methods

        private void GetUserInfo()
        {

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {
                try
                {

                    User user = await AysaClient.Instance.GetUserInfo();

                    InvokeOnMainThread(() =>
                    {
                        UserSession.Instance.Id = user.Id;
                        UserSession.Instance.UserName = user.UserName;
                        UserSession.Instance.nomApel = user.NombreApellido;
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
                        //ShowErrorAlert("Error al obtener la información del usuario.");
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

                }
            });

        }

        private User GetUserLogged()
        {
            User user = new User() 
            {
                Id = UserSession.Instance.Id,
                UserName = UserSession.Instance.UserName,
                NombreApellido = UserSession.Instance.nomApel,
            };

            return user;
        }

        private async Task SetUpInputIBOutlets()
        {

            // Add tap gesture recognizer to dismiss the keyboard
            View.AddGestureRecognizer(new UITapGestureRecognizer(() => this.DismissKeyboard()));


            // Add left padding in NumberEventTextField
            UIView paddingDateView = new UIView(new CGRect(0, 0, 30, 0));
            DateTextField.LeftView = paddingDateView;
            DateTextField.LeftViewMode = UITextFieldViewMode.Always;
            User usuario = GetUserLogged();
            AutorTextField.Text = string.IsNullOrEmpty(usuario.NombreApellido) ? usuario.UserName : usuario.NombreApellido;
            AutorTextField.UserInteractionEnabled = false;

            await LoadPickerViewInDateTextFields();

            // For some reason the event TouchUpInside don't response if it's assign from .Storyboard
            CreateButton.TouchUpInside += CreateEventAction;
            UploadFileButton.TouchUpInside += SelectFileAction;

            // Int ImagePickerViewController
            ImagePicker = new UIImagePickerController();
            ImagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            ImagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
            ImagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
            ImagePicker.Canceled += Handle_Canceled;

            DocumentPicker = new UIDocumentPickerViewController(allowedUTIs, UIDocumentPickerMode.Import);
            DocumentPicker.WasCancelled += Picker_WasCancelled;
            DocumentPicker.DidPickDocumentAtUrls += Handle_DidPickDocumentAtUrls;

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
                        AttachmentFile attachmentFile = new AttachmentFile();
                        attachmentFile.FileName = nameFile;
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
            LoadAttachmentsOfEvent();
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

        private async Task LoadPickerViewInDateTextFields()
        {
            // Load datePicker for DateTextField
            // Format NSDate
            NSDateFormatter dateFormat = new NSDateFormatter();
            dateFormat.DateFormat = "yyyy-MM-dd HH:mm";

            var dateNow = DateTime.Now;
            PickerFromDate = new UIDatePicker();
            PickerFromDate.Mode = UIDatePickerMode.DateAndTime;
            PickerFromDate.Date = (NSDate)dateNow;

            string idEvent = null;
            if (EditFeature != null && FirstLoadDate)
            {
                FirstLoadDate = false;
                idEvent = EditFeature.Sector.Id;
            }
            await UpdateDateBySection(idEvent);

            DateTime deadline = MinorDate();
            if (!roles.Any(r => r.Nombre == "Referente PPE") && roles.Any(r => r.Nombre == "Responsable Guardia"))
            {
                PickerFromDate.MinimumDate = (NSDate)semana.Desde;
                PickerFromDate.MaximumDate = (NSDate)deadline;
            }
            PickerFromDate.ValueChanged += TextFieldDateFromValueChanged;
            PickerFromDate.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
            DateTextField.InputView = PickerFromDate;
            DateTextField.Text = dateFormat.ToString(PickerFromDate.Date);
        }



        private void LoadPickerViewInSectionTextField()
        {
            List<Section> SectorGuardList = UserSession.Instance.PersonInGuard.Sectores;
            // Check Active Sections have values
            if (ActiveSectionsList.Count == 0)
            {
                SectionEventTextField.Enabled = false;
                SectionEventTextField.BackgroundColor = UIColor.FromRGB(220, 220, 220);
                return;
            }

            // Load Picker for TypeTextField
            UIPickerView picker = new UIPickerView();
            // Build data from list of EventsType
            List<string> data = new List<string>();
            int indexSelect = 0;

            for (int i = 0; i < ActiveSectionsList.Count; i++)
            {
                Section sectionObj = ActiveSectionsList[i];
                data.Add(string.Format("{0}", sectionObj.Nombre));
                if (SectorGuardList.Count > 0 && SectorGuardList.Any(x => x.Id == sectionObj.Id))
                {
                    indexSelect = i;
                    SectionEventTextField.Text = sectionObj.Nombre;
                    SectionSelected = sectionObj;

                }
                //data.Add(string.Format("{0} - Nivel: {1}", sectionObj.Nombre, sectionObj.Nivel));
            }

            PickerTextFieldDataSource modelPicker = new PickerTextFieldDataSource(data, SectionEventTextField);
            modelPicker.Delegate = this;
            picker.Model = modelPicker;
            picker.Select(indexSelect, 0, true);

            SectionEventTextField.InputView = picker;

        }

        private void PrepareViewToEditEvent()
        {
            // Load IBOutlets with values of event that it will be edited
            LoadEventWillBeEditedInView();

            CreateButton.SetTitle("Modificar", UIControlState.Normal);

            this.NavigationItem.Title = "Editar Novedad";

        }

        private void LoadEventWillBeEditedInView()
        {
            User usuario = GetUserLogged();
            // Format NSDate
            NSDateFormatter dateFormat = new NSDateFormatter();
            dateFormat.DateFormat = "yyyy-MM-dd HH:mm";

            TitleTextView.Text = EditFeature.Detail;
            DateTextField.Text = dateFormat.ToString((NSDate)EditFeature.Date);
            SectionSelected = EditFeature.Sector;
            AutorTextField.Text = EditFeature.Usuario.NombreApellido;
            SectionEventTextField.Text = EditFeature.Sector.Nombre;

            // Set Character Counter value on Edit 
            CounterDetailText.Text = $"{EditFeature.Detail.Length}/{detailLimit}";
            IsMaxCharactersTextView(TitleTextView, (detailLimit - 5));

            // Load AttachmentContentView with files of event
            AttachmentFilesInMemory = new List<AttachmentFile>();
            AttachmentFilesInMemory.AddRange(EditFeature.Archivos);

        }


        private void DismissKeyboard()
        {
            View.EndEditing(true);
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

        private bool IsEmptyTextField(UITextField textField)
        {
            if (textField.Text.Length == 0)
            {

                textField.Layer.BorderColor = UIColor.Red.CGColor;
                textField.Layer.BorderWidth = 1.0f;

                return true;
            }
            else
            {
                textField.Layer.BorderColor = UIColor.Clear.CGColor;
                textField.Layer.BorderWidth = 1.0f;

                return false;
            }
        }

        private bool IsInvalidFutureDate(UITextField DateTextField)
        {
            DateTime dateSelected = Convert.ToDateTime(DateTextField.Text);

            DateTime currently = DateTime.Now;

            bool isInvalid = dateSelected > currently;

            return isInvalid;
        }

        private bool IsInvalidMinDate(UITextField DateTextField)
        {
            DateTime dateSelected = Convert.ToDateTime(DateTextField.Text);

            bool isInvalid = dateSelected < semana.Desde;

            return isInvalid;
        }

        private bool IsInvalidMaxDate(UITextField DateTextField)
        {
            DateTime dateSelected = Convert.ToDateTime(DateTextField.Text);

            bool isInvalid = dateSelected > semana.Hasta;

            return isInvalid;
        }

        private bool IsPastLimitDate()
        {
            DateTime currently = DateTime.Now;

            bool isInvalid = currently > semana.Hasta && currently.Hour >= 17;

            return isInvalid;
        }

        private DateTime MinorDate()
        {
            DateTime currently = DateTime.Now;
            if (currently > semana.Hasta)
            {
                return semana.Hasta;
            }
            else
            {
                return currently;
            }
        }

        private bool IsEmptyTextView(UITextView textView)
        {
            if (textView.Text.Length == 0)
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

        private bool IsObjectEmpty(Object objectValue, UITextField textView)
        {
            if (objectValue == null)
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


        private string ValidateInputFields()
        {

            // Validate Titulo
            if (IsEmptyTextView(TitleTextView))
            {
                // Throw error message
                return "El campo Detalle no puede estar  vacío";
            }

            // Validate Fecha de Ocurrencia
            if (IsEmptyTextField(DateTextField))
            {
                // Throw error message
                return "El campo Fecha de Ocurrencia no puede estar  vacío";
            }

            if (!roles.Any(r => r.Nombre == "Referente PPE") &&
                roles.Any(r => r.Nombre == "Responsable Guardia"))
            {
                //Validate Date - future
                if (IsInvalidFutureDate(DateTextField))
                {
                    return "No puede cargar novedades con día o horario posterior al actual.";
                }

                //Validate Date - out of guard - past
                if (IsInvalidMinDate(DateTextField))
                {
                    return "No puede cargar novedades con día o horario anterior al inicio de la guardia.";
                }

                //Validate Date - out of guard - future
                if (IsInvalidMaxDate(DateTextField))
                {
                    return "No puede cargar novedades con día o horario posterior al de finalización de la guardia.";
                }

                //Validate Date - out of guard - Publication after 17hs
                if (IsPastLimitDate())
                {
                    return "No puede cargar novedades luego de las 17hs una vez finalizada la guardia.";
                }
            }   

            // Validate Section
            if (IsObjectEmpty(SectionSelected, SectionEventTextField))
            {
                // Throw error message
                return "Es necesario seleccionar un Sector";
            }


            return "";
        }

        private bool IsMaxCharactersTextView(UITextView textView, int limit)
        {
            // Cambia el color del borde del area de texto al llegar al limite
            if (textView.Text.Length > limit)
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


        private Feature BuildEventWillCreateFromUI()
        {

            // Validate Input fields
            string validateErrorMessage = ValidateInputFields();
            if (validateErrorMessage.Length > 0)
            {
                // There are errors in the Input fields.. show Alert
                ShowErrorAlert(validateErrorMessage);
                BTProgressHUD.Dismiss();
                return null;
            }


            // Create Event
            if (IsMaxCharactersTextView( TitleTextView, detailLimit ))
            {
                ShowErrorAlert($"Para el campo Detalle:\nLímite de {detailLimit} caracterres");
                BTProgressHUD.Dismiss();
            }
            Feature eventObj = new Feature();
            eventObj.Id = EditFeature != null ? EditFeature.Id : null;
            eventObj.CanEdit = EditFeature != null ? EditFeature.CanEdit : false;
            eventObj.Detail = TitleTextView.Text;
            eventObj.Date = Convert.ToDateTime(DateTextField.Text);
            eventObj.Sector = SectionSelected;
            eventObj.Usuario = GetUserLogged();

            if (EditFeature != null)
            {
                eventObj.Archivos = EditFeature.Archivos;

                if (filesToDeleteInServer.Count > 0)
                {
                    foreach (AttachmentFile file in filesToDeleteInServer)
                    {
                        eventObj.Archivos.Remove(file);
                    }
                }

                // Concatenate new files
                eventObj.Archivos.AddRange(UploadedAttachmentFiles);
            }
            else
            {
                eventObj.Archivos = UploadedAttachmentFiles;
            }



            return eventObj;
        }

        private void SendEventToServer(Feature eventObj)
        {

            Task.Run(async () =>
            {

                try
                {
                    Feature featureCreated;

                    if (EditFeature == null)
                    {
                        // Create Event
                        featureCreated = await AysaClient.Instance.CreateFeature(eventObj);
                    }
                    else
                    {
                        // Update Event
                        featureCreated = await AysaClient.Instance.UpdateFeature(EditFeature.Id, eventObj);
                    }

                    featureCreated.CanEdit = eventObj.CanEdit;

                    InvokeOnMainThread(() =>
                    {
                        if (EditFeature != null)
                        {
                            // Send filter to EventsViewController to get events by filter
                            if (_delegate != null)
                                Delegate?.FeatureWasUpdated(featureCreated);
                        }

                        string successMsj = EditFeature == null ? "La novedad ha sido creado con éxito" : "La novedad ha sido modificado con éxito";
                        BTProgressHUD.ShowImage(UIImage.FromBundle("ok_icon"), successMsj, 2000);
                        PerformSelector(new Selector("PopViewController"), null, 2.0f);
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
                        //ShowErrorAlert("Error al enviar la novedad al servidor.");
                        CountAttachmentsUploaded = 0;
                    });
                }
            });
        }

        [Export("PopViewController")]
        void PopViewController()
        {
            // Remove progress
            BTProgressHUD.Dismiss();
            this.NavigationController.PopViewController(true);
        }


        public void LoadActiveSectionsInView()
        {

            ActiveSectionsList = UserSession.Instance.ActiveSections;
            // After get Sections Active list, load elements in PickerView
            LoadPickerViewInSectionTextField();
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
                        if(CountAttachmentsUploaded < AttachmentFilesInMemory.Count)
                        {
                            UploadFilesToServer();
                        }
                        else
                        {
                            InvokeOnMainThread(() =>
                            {
                                Feature eventObj = BuildEventWillCreateFromUI();

                                if (eventObj != null)
                                {
                                    SendEventToServer(eventObj);
                                }
                            });
                        }

                        return;
                    }

                    AttachmentFile fileUpload = await AysaClient.Instance.UploadFileFeature(fileInMemory.BytesArray, fileInMemory.FileName);

                    //AttachmentFile fileUpload = await AysaClient.Instance.UploadFile(attachmentFile.BytesArray, attachmentFile.FileName);

                    InvokeOnMainThread(() =>
                    {
                        // Save uploaded file in list, this list will be assigned to the event that it will be created
                        UploadedAttachmentFiles.Add(fileUpload);

                        CountAttachmentsUploaded++;

                        if (CountAttachmentsUploaded < AttachmentFilesInMemory.Count)
                        {

                            UploadFilesToServer();
                        }
                        else
                        {
                            Feature eventObj = BuildEventWillCreateFromUI();

                            if (eventObj != null)
                            {
                                SendEventToServer(eventObj);
                            }
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
                        //ShowErrorAlert("Error al cargar archivos al servidor.");
                        // Remove progress
                        BTProgressHUD.Dismiss();

                        CountAttachmentsUploaded = 0;
                    });
                }

            });

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
            for (int i = 0; i < AttachmentFilesInMemory.Count; i++)
            {
                // Create observation view
                AttachmentFileView attachmentView = AttachmentFileView.Create();

                // Get top position of attachment file
                int topPosition = i * HeightAttachmentView;
                attachmentView.Frame = new CGRect(0, topPosition, AttachmentContentView.Frame.Width, HeightAttachmentView);
                attachmentView.Delegate = this;

                // Load file object in View
                attachmentView.LoadAttachmentFileInView(AttachmentFilesInMemory[i], false);

                // Add attachment in Content View
                AttachmentContentView.AddSubview(attachmentView);
            }
        }

        private void AdjustSizeAttachmentContentView()
        {
            // Config AttachmentContent according to there are attachments or not
            if (AttachmentFilesInMemory.Count > 0)
            {
                AttachmentContentView.Hidden = false;
                TopAttachmentContentConstraint.Constant = 10;
                View.LayoutIfNeeded(); ;
            }
            else
            {
                AttachmentContentView.Hidden = true;
                TopAttachmentContentConstraint.Constant = 0;
                View.LayoutIfNeeded(); ;
            }

            // Set Height of AttachmentContentView according to count of files in Event
            HeightAttachmentContentConstraint.Constant = AttachmentFilesInMemory.Count * HeightAttachmentView;
            View.LayoutIfNeeded();
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
                    LoadAttachmentsOfEvent();

                }, delegate (NSError error) {
                    return;
                });
            }
        }

        private void IntSecuenceToUploadEventInServer()
        {

            //Show a HUD with a progress spinner and the text
            BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

            if (AttachmentFilesInMemory.Count > 0)
            {
                UploadFilesToServer();
            }
            else
            {
                Feature eventObj = BuildEventWillCreateFromUI();

                objectGlobal = eventObj;

                if (eventObj != null)
                {
                    SendEventToServer(eventObj);
                }
            }

        }

        #endregion

        #region Implement PickerTextFieldDataSourceDelegate Metods

        public async void ItemSelectedValue(int indexSelected, UITextField textField)
        {

            // TextField with Tag value 1 = TypeEventTextField
            switch (textField.Tag)
            {
                case 2:
                    SectionSelected = ActiveSectionsList[indexSelected];
                    SectionEventTextField.Text = string.Format("{0}", SectionSelected.Nombre);
                    //SectionEventTextField.Text = string.Format("{0} - Nivel: {1}", SectionSelected.Nombre, SectionSelected.Nivel);
                    break;
                default:
                    break;
            }
            await LoadPickerViewInDateTextFields();
        }

        #endregion

        #region Implement UIImagePickerViewController Delegate

        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs media)
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

            // Show file selected in view
            LoadAttachmentsOfEvent();
            ImagePicker.DismissViewController(true, null);
        }

        void Handle_Canceled(object sender, EventArgs e)
        {
            ImagePicker.DismissViewController(true, null);
        }

        #endregion

        #region Implement Delegates of AttachmentFileViewDelegate 

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
                            AttachmentFile fileInMemory = AttachmentFilesInMemory[index];
                            InvokeOnMainThread(() =>
                            {
                                BTProgressHUD.Dismiss();
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
                        //ShowErrorAlert("Error al descargar los archivos.");
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    BTProgressHUD.Dismiss();
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

            // Delete file from server if it's necessary 
            // If the file has id that means that it was upload to the server, so it's needed to remove
            if (documentFile.Id != null)
            {
                // Call WS to remove file in server
                filesToDeleteInServer.Add(documentFile);
                RemoveFileFromServer(documentFile);
            }
            else
            {
                // It's not necessary remove file in the server, so only remove in view
                RemoveFileFromView(documentFile);
            }
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
                LoadAttachmentsOfEvent();
            }
        }

        private void RemoveFileFromServer(AttachmentFile file)
        {

            //Show a HUD with a progress spinner and the text
            BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

            Task.Run(async () =>
            {

                try
                {
                    AttachmentFile response = await AysaClient.Instance.DeleteFile(file.Id);

                    InvokeOnMainThread(() =>
                    {
                        string successMsj = "El evento ha sido eliminado";
                        BTProgressHUD.ShowImage(UIImage.FromBundle("ok_icon"), successMsj, 2000);

                        // Remove file in view
                        RemoveFileFromView(file);
                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowErrorAlert("No tiene permisos para ejecutar esta acción");
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                        //ShowErrorAlert("Error al eliminar archivos del servidor.");
                    });
                }
                finally
                {
                    BTProgressHUD.Dismiss();
                }
            });
        }

        #endregion

        #region IBActions Methods

        public void TextFieldDateFromValueChanged(object sender, EventArgs e)
        {

            // Format NSDate
            NSDateFormatter dateFormat = new NSDateFormatter();
            dateFormat.DateFormat = "yyyy-MM-dd HH:mm";

            DateTime fromDate = (DateTime)PickerFromDate.Date;

            if(!roles.Any(r => r.Nombre == "Referente PPE") &&
                roles.Any(r => r.Nombre == "Responsable Guardia") &&
                (fromDate < semana.Desde || fromDate > semana.Hasta))

            {
                ShowErrorAlert("Fecha seleccionada inválida. Seleccione fecha dentro de la semana activa.");
            }

            var localTime = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc).ToLocalTime();
            DateTextField.Text = dateFormat.ToString(PickerFromDate.Date);
        }


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

        #region Old Create Event Action Feature
        //public void CreateEventAction(object sender, EventArgs e)
        //{
        //    if (EditFeature != null)
        //    {
        //        if (EditFeature.Usuario.NombreApellido == AutorTextField.Text)
        //        {
        //            IntSecuenceToUploadEventInServer();
        //        }
        //        else
        //        {
        //            string errorMss = "No puedes modificar una novedad de la que no eres autor.";
        //            var alert = UIAlertController.Create("Error", errorMss, UIAlertControllerStyle.Alert);
        //            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
        //            PresentViewController(alert, true, null);
        //        }
        //    }
        //    else
        //    {
        //        IntSecuenceToUploadEventInServer();
        //    }
        //}
        #endregion

        public void CreateEventAction(object sender, EventArgs e)
        {
            IntSecuenceToUploadEventInServer();
        }



        #endregion
    }
}

