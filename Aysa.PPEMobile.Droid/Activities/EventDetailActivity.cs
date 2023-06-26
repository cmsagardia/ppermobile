using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using AndroidX.AppCompat.App;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Droid.Utilities;
using Android.Text;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using AndroidX.AppCompat.Content;
using Android.Support.V4.App;
using Android.Content.PM;
using Android;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using Xamarin.Essentials;
using Java.IO;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "EventDetailActivity")]
    public class EventDetailActivity : AppCompatActivity
    {
        FrameLayout progressOverlay;
        private Event mEvent;
        private TextView tvEventTitle;
        private TextView tvDetailDate;
        private TextView tvDetailPlace;
        private TextView tvDetailType;
        private TextView tvDetailDetail;
        private TextView tvDetailStatus;
        private TextView tvDetailTags;
        private TextView tvUserCreator;
        private TextView tvSector;
        private LinearLayout eventsDocumentList;
        private CheckBox confidentialCheckBox;
        private CheckBox privateCheckBox;
        private TextView privateTag;
        MultilineEditText editTextEvDetailGeneral;
        Spinner spinnerSectors;
        List<Section> ActiveSectionsList;
        readonly int EDIT_EVENT_ACTIVITY_CODE = 90;
        Android.Net.Uri pdfpath;
        LinearLayout filesContainer;

        bool showEditButton;
        bool clickEditButton = false;

        //used if this event was edited through AddEventActivity
        private bool editedEvent = false;

        List<AttachmentFile> attachedNoteFiles = new List<AttachmentFile>();
        List<AttachmentFile> uploadedAttachmentFiles = new List<AttachmentFile>();
        int countAttachmentsUploaded = 0;
        int countAttachmentsDeleted = 0;
        readonly int limitObservacion = 1000;

        List<AttachmentFile> filesNoteToDeleteInServer = new List<AttachmentFile>();
        LinearLayout filesNoteContainer;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.mEvent = EventDataHolder.getInstance().getData();
            showEditButton = mEvent.CanEdit;

            setUpViews();

            LoadActiveSectionsInView();

            LoadFullEvent(this.mEvent);

            //SetUpViewAccordingUserPermissions();

        }

        private void setUpViews()
        {
            SetContentView(Resource.Layout.EventDetail);

            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            // Set toolbar and title
            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.Title = "Evento #" + mEvent.NroEvento.ToString();

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            tvEventTitle = FindViewById<TextView>(Resource.Id.textViewEvDetailTitle);
            tvDetailDate = FindViewById<TextView>(Resource.Id.textViewEvDetailDate);
            tvDetailPlace = FindViewById<TextView>(Resource.Id.textViewEvDetailPlace);
            tvDetailType = FindViewById<TextView>(Resource.Id.textViewEvDetailType);
            tvDetailDetail = FindViewById<TextView>(Resource.Id.textViewEvDetailDetalle);
            tvDetailStatus = FindViewById<TextView>(Resource.Id.textViewEvDetailStatus);
            tvDetailTags = FindViewById<TextView>(Resource.Id.textViewEvDetailTags);
            confidentialCheckBox = FindViewById<CheckBox>(Resource.Id.confidentialCheckBox);
            privateTag = FindViewById<TextView>(Resource.Id.privateTextView); 
            privateCheckBox = FindViewById<CheckBox>(Resource.Id.checkbox);
            editTextEvDetailGeneral = FindViewById<MultilineEditText>(Resource.Id.editTextEvDetailGeneral);
            eventsDocumentList = FindViewById<LinearLayout>(Resource.Id.eventsDocumentList);
            tvUserCreator = FindViewById<TextView>(Resource.Id.textViewEvUserCreator);
            tvSector = FindViewById<TextView>(Resource.Id.textViewEvSector);
            // Sector field
            spinnerSectors = FindViewById<Spinner>(Resource.Id.spinnerSectors);

            Button btnAddNote = FindViewById<Button>(Resource.Id.btnEventDetailSendGeneral);
            btnAddNote.Click += BtnAddNote_Click;

            confidentialCheckBox.Click += Confidential_Checked_Click;

            editTextEvDetailGeneral.AfterTextChanged += ObservationDetailText_Changed;

            // Observation files
            filesNoteContainer = FindViewById<LinearLayout>(Resource.Id.addEventFilesContainerObservation);
            FindViewById<Button>(Resource.Id.btnUploadFilesObservation).Click += UploadFileNote_Click;
        }

        async void UploadFileNote_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                {
                    if (!ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage))
                    {
                        //Don't ask again checked

                        Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                        Android.App.AlertDialog alert = dialog.Create();
                        alert.SetTitle("¡Advertencia!");
                        alert.SetMessage("La App no tiene acceso al almacenamiento, fue denegado por el usuario. Si quiere autorizar diríjase a Configuración > Aplicaciones y verifique los permisos sobre la App PPEMobile");
                        alert.SetButton2("OK", (c, ev) => { });
                        alert.Show();
                    }
                    else
                        ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, 1);
                }
                else 
                {

                    var customFileType =
                        new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                         { DevicePlatform.Android, new[]
                         { "application/pdf",
                           "application/msword","application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .doc & .docx,
                           "application/vnd.ms-PowerPoint","application/vnd.openxmlformats-officedocument.presentationml.presentation", // .ppt & .pptx
                           "application/vnd.ms-Excel","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xls & .xlsx
                           "application/zip",
                           "image/*",
                           "video/*",
                           "text/plain",
                           "application/rar"
                         }
                    }});

                    var pickResult = await FilePicker.PickAsync(new PickOptions
                    {
                        //FileTypes = FilePickerFileType.Images,
                        FileTypes = customFileType,
                        PickerTitle = "Seleccionar Archivo"
                    });

                    if (pickResult != null)
                    {
                        ByteArrayOutputStream baos = new ByteArrayOutputStream();
                        FileInputStream fis;

                        string filename = pickResult.FileName;

                        System.IO.FileInfo fileSelected = new System.IO.FileInfo(pickResult.FullPath);
                        long size = fileSelected.Length;
                        //20971520 -> Limite de 20MB
                        //Ajustado para que sean 10 MB
                        if (size <= 10971520)
                        {
                            AttachmentFile file = new AttachmentFile(filename);
                            fis = new FileInputStream(new Java.IO.File(pickResult.FullPath));

                            byte[] buf = new byte[1024];
                            int n;
                            while (-1 != (n = fis.Read(buf)))
                                baos.Write(buf, 0, n);

                            Byte[] fileByteArray = baos.ToByteArray();
                            file.BytesArray = fileByteArray;

                            attachedNoteFiles.Add(file);
                            AddAttachedNoteFile(file);
                        }
                        else
                        {
                            ShowErrorAlert("El archivo que intenta subir supera los 10 Mb permitidos");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorAlert("Ocurrió un Error al intentar subir el archivo.");
            }
        }

        private void DeleteNoteFile(AttachmentFile documentFile, int filePosition)
        {

            // Delete file from server if it's necessary 
            // If the file has id that means that it was upload to the server, so it's needed to remove
            if (documentFile.Id != null)
            {
                filesNoteToDeleteInServer.Add(documentFile);

                // Call WS to remove file in server
                //RemoveFileFromServer(documentFile, filePosition);
            }

            // Remove file in view
            attachedNoteFiles.RemoveAt(filePosition);

            filesNoteContainer.RemoveViewAt(filePosition);

        }

        private void ClickRemoveFileNote(object sender, EventArgs e)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            alert.SetMessage("¿Está seguro que desea quitar el Archivo?");
            alert.SetButton("Si", (c, ev) =>
            {
                ViewGroup fileRow = (ViewGroup)((View)sender).Parent;
                int filePosition = filesNoteContainer.IndexOfChild(fileRow);

                DeleteNoteFile(attachedNoteFiles[filePosition], filePosition);

            });

            alert.SetButton2("Cancelar", (c, ev) => { });

            alert.Show();


        }

        private void AddAttachedNoteFile(AttachmentFile file)
        {
            string privatepublic = "Publico";
            //if(privateCheckBox.Checked)
            //{
            //    privatepublic = "Privado";
            //}
            //else
            //{
            //    privatepublic = "Publico";
            //}
            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
            ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.AddEventDocumentRow, filesNoteContainer, false);
            view.FindViewById<TextView>(Resource.Id.txtDocumentName).Text = file.FileName;
            view.FindViewById<TextView>(Resource.Id.txtDocumentName).PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
            view.FindViewById<ImageView>(Resource.Id.btnRemoveEventDocument).Click += ClickRemoveFileNote;

            filesNoteContainer.AddView(view);

            view.Click += (sender, e) => OnDocumentClick(this, file);
        }

        void BtnAddNote_Click(object sender, System.EventArgs e)
        {
            if (editTextEvDetailGeneral.Text.Length == 0 && attachedNoteFiles.Count == 0)
            {
                ShowErrorAlert($"La observación debe tener un comentario o un adjunto cargado!");
            }
            else
            {
                Observation observationObj = BuildObservation();

                if (observationObj.Observacion.Length > limitObservacion)
                {
                    string message = $"Para el campo General: \nSupero limite de {limitObservacion} caracterres";
                    global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
                    alert.SetTitle("Aviso");
                    alert.SetMessage(message);
                    alert.SetPositiveButton("Ok", CloseAction);

                    Dialog dialog = alert.Create();
                    dialog.Show();
                    return;
                }
                ShowProgress(true);
                if (attachedNoteFiles.Count > 0)
                {
                    UploadFilesToServer();
                }
                else
                {
                    UploadObservationToServer();
                }
            }
        }

        private void ObservationDetailText_Changed(object sender, EventArgs e)
        {
            if (editTextEvDetailGeneral.Text.Length == 1000)
            {
                global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
                alert.SetTitle("Aviso");
                alert.SetMessage($"Para el campo Detalle:\nAlcanzó el límite de {limitObservacion} caracterres");
                alert.SetPositiveButton("Ok", CloseAction);

                Dialog dialog = alert.Create();
                dialog.Show();
                return;
            }
        }

        void UploadObservationToServer()
        {
            Observation observationObj = BuildObservation();

            if (observationObj != null)
            {
                SendObservationToServer(observationObj);
            }
        }

      
        private void UploadFilesToServer()
        {
            // Upload files

            Task.Run(async () =>
            {

                try
                {
                    AttachmentFile fileInMemory = attachedNoteFiles[countAttachmentsUploaded];

                    // If Attachment file doesn't have file that means that it's already added
                    if (fileInMemory.BytesArray == null)
                    {
                        countAttachmentsUploaded++;
                        if (countAttachmentsUploaded < attachedNoteFiles.Count())
                        {
                            UploadFilesToServer();
                        }
                        else
                        {
                            UploadObservationToServer();
                        }

                        return;
                    }

                    AttachmentFile fileUpload = await AysaClient.Instance.UploadFileBase64(fileInMemory.BytesArray, fileInMemory.FileName, fileInMemory.Private);

                    //AttachmentFile fileUpload = await AysaClient.Instance.UploadFile(attachmentFile.BytesArray, attachmentFile.FileName);

                    RunOnUiThread(() =>
                    {                                           
                        // Save uploaded file in list, this list will be assigned to the event that it will be created
                        uploadedAttachmentFiles.Add(fileUpload);

                        countAttachmentsUploaded++;

                        if (countAttachmentsUploaded < attachedNoteFiles.Count)
                        {

                            UploadFilesToServer();
                        }
                        else
                        {
                            UploadObservationToServer();
                        }

                    });

                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        // Remove progress
                        ShowProgress(false);
                        

                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }

            });

        }

        void Confidential_Checked_Click(object sender, System.EventArgs e)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            string message = mEvent.Confidencial ? "¿Está seguro que desea desmarcar el evento como confidencial?" : "¿Está seguro que desea marcar el evento como confidencial?";
            alert.SetMessage(message);
            alert.SetButton("Si", (c, ev) =>
            {
                ChangeEventStatusCenfidential();
            });

            alert.SetButton2("Cancelar", (c, ev) => { });

            alert.Show();
        }

        private Observation BuildObservation()
        {
            Observation observationObj = new Observation();
            observationObj.Fecha = DateTime.Now;
            observationObj.Observacion = editTextEvDetailGeneral.Text;
            observationObj.Evento = GetEventAssociated();
            observationObj.Usuario = GetUserLogged();
            observationObj.Directorio = false;
            observationObj.Archivos = uploadedAttachmentFiles;

            if (privateCheckBox.Checked)
            {
                observationObj.Directorio = true;
            }

            if (FindViewById(Resource.Id.spinnerSectorContainer).Visibility == ViewStates.Gone)
            {
                observationObj.Sector = mEvent.Sector;
            }
            else
            {
                observationObj.Sector = ActiveSectionsList[spinnerSectors.SelectedItemPosition];
            }

            return observationObj;
        }

        //SECTOR
        public void LoadActiveSectionsInView()
        {
            ActiveSectionsList = UserSession.Instance.ActiveSections;

            var roles = UserSession.Instance.Roles;
            var personGuard = UserSession.Instance.PersonInGuard;
            var sections = personGuard.Sectores;

            if (mEvent.CanEdit)
            {
                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, ActiveSectionsList.ToArray());
                spinnerSectors.Adapter = adapter;
            }
            else
            {
                FindViewById(Resource.Id.spinnerSectorContainer).Visibility = ViewStates.Gone;
                FindViewById(Resource.Id.addObservationContent).Visibility = ViewStates.Invisible;
                FindViewById(Resource.Id.titleObservations).Visibility = ViewStates.Invisible;
                FindViewById(Resource.Id.separationLine).Visibility = ViewStates.Invisible;
            }

            if (roles.Any(r => r.Nombre.Equals("Directorio")))
            {
                if(roles.Any(r => r.Nombre.Equals("Referente PPE")))
                    FindViewById(Resource.Id.spinnerSectorContainer).Visibility = ViewStates.Visible;
                else
                    FindViewById(Resource.Id.spinnerSectorContainer).Visibility = ViewStates.Gone;

                FindViewById(Resource.Id.editTextEvDetailGeneral).Enabled = true;
                FindViewById(Resource.Id.btnUploadFilesObservation).Enabled = true;
                FindViewById(Resource.Id.btnEventDetailSendGeneral).Enabled = true;
            }

            if (roles.Any(r => r.Nombre.Equals("Referente PPE")) || sections.Any(x => x.Nivel == 1))
                FindViewById(Resource.Id.confidentialContent).Visibility = ViewStates.Visible;
            else
                FindViewById(Resource.Id.confidentialContent).Visibility = ViewStates.Gone;

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
            eventObservation.Id = mEvent.Id;

            return eventObservation;
        }

        private void SendObservationToServer(Observation observationObj)
        {
            Task.Run(async () =>
            {
                try
                {
                    Observation observation = await AysaClient.Instance.CreateObservation(observationObj);

                    RunOnUiThread(() =>
                    {
                        ShowProgress(false);
                        if (observation != null)
                        {
                            if (mEvent.Observaciones != null)
                            {
                                editTextEvDetailGeneral.Text = "";
                                mEvent.Observaciones.Insert(0, observation);

                                //Limpiar label de adjunto
                                filesNoteContainer.RemoveAllViews();
                                //Limpiar listas luego de la carga de adjunto
                                attachedNoteFiles = new List<AttachmentFile>();
                                uploadedAttachmentFiles = new List<AttachmentFile>();
                                countAttachmentsUploaded = 0;
                                // Add observation in fist position to show it in first place
                                ShowEventNotes();
                            }
                        }

                        string successMsj = "La observación ha sido creada con éxito";

                        string toast = string.Format(successMsj);
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                //catch (HttpUnauthorized)
                //{
                //    RunOnUiThread(() =>
                //    {
                //        ShowProgress(false);

                //        ShowSessionExpiredError();
                //    });
                //}
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        ShowProgress(false);
                        ShowErrorAlert(ex.Message);
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });
        }

        private void ChangeEventStatusCenfidential()
        {
            ShowProgress(true);

            Task.Run(async () =>
            {

                try
                {
                    await AysaClient.Instance.SetEventConfidential(mEvent.Id);

                    RunOnUiThread(() =>
                    {
                        mEvent.Confidencial = !mEvent.Confidencial;
                        confidentialCheckBox.Checked = mEvent.Confidencial;
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });

            ShowProgress(false);
        }


        //private void ShowSessionExpiredError()
        //{
        //    global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
        //    alert.SetTitle("Aviso");
        //    alert.SetMessage("Su sesión ha expirado, por favor ingrese sus credenciales nuevamente");

        //    Dialog dialog = alert.Create();
        //    dialog.Show();
        //}

        public async void LoadFullEvent(Event eventSelected)
        {
            ShowProgress(true);
            // Get complete data of event selected from server
            await Task.Run(async () =>
            {
                try
                {
                    mEvent = await AysaClient.Instance.GetEventById(eventSelected.Id);
                    mEvent.Archivos = await AysaClient.Instance.GetFilesOfEvent(mEvent.Id);               
                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowProgress(false);
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });

            // Go to event detail
            ShowEventData();

            EventDataHolder.getInstance().setData(mEvent);
            if (mEvent.Archivos.Count > 0)
            {
                ShowEventDocuments();
            }
            ShowProgress(false);
            clickEditButton = true;
        }

        private void ShowEventData()
        {
            //tvUserCreator.Text = mEvent.Usuario.NombreApellido;

            tvEventTitle.Text = mEvent.Titulo;

            String sourceString1 = "<b>Fecha de ocurrencia:</b> " + mEvent.Fecha.ToString(AysaConstants.FormatDateTime);
            tvDetailDate.TextFormatted = Html.FromHtml(sourceString1, FromHtmlOptions.ModeLegacy);

            String sourceString2 = "<b>Lugar:</b> " + mEvent.Lugar;
            tvDetailPlace.TextFormatted = Html.FromHtml(sourceString2, FromHtmlOptions.ModeLegacy);

            String sourceString3 = "<b>Tipo:</b> " + mEvent.Tipo;
            tvDetailType.Visibility = ViewStates.Visible;
            tvDetailType.TextFormatted = Html.FromHtml(sourceString3, FromHtmlOptions.ModeLegacy);

            String sourceString4 = "<b>Detalle:</b> " + mEvent.Detalle;
            tvDetailDetail.Visibility = ViewStates.Visible;
            tvDetailDetail.TextFormatted = Html.FromHtml(sourceString4, FromHtmlOptions.ModeLegacy);
             

            String sourceString5 = "<b>Estado:</b> " + (mEvent.Estado == 1 ? "Abierto" : "Cerrado");
            tvDetailStatus.TextFormatted = Html.FromHtml(sourceString5, FromHtmlOptions.ModeLegacy);

            String sourceString6 = "<b>Usuario Creador:</b> " + mEvent.Usuario.NombreApellido;
            tvUserCreator.TextFormatted = Html.FromHtml(sourceString6, FromHtmlOptions.ModeLegacy);


            String sourceString7 = "<b>Sector:</b> " + mEvent.Sector;
            tvSector.TextFormatted = Html.FromHtml(sourceString7, FromHtmlOptions.ModeLegacy);

            
                //String sourceString6 = mEvent.Tag;
            //if (mEvent.Tag == null || mEvent.Tag.Trim().Equals(""))
            //{
            //    tvDetailTags.Visibility = ViewStates.Gone;
            //}
            //else
            //{
            //    tvDetailTags.Visibility = ViewStates.Visible;
            //    tvDetailTags.TextFormatted = Html.FromHtml(sourceString6, FromHtmlOptions.ModeLegacy);
            //}


            TextView generalTextView = FindViewById<TextView>(Resource.Id.general_textView);
            generalTextView.TextFormatted = Html.FromHtml("<b>General</b> ", FromHtmlOptions.ModeLegacy);
            TextView sectionTextView = FindViewById<TextView>(Resource.Id.section_textView);
            sectionTextView.TextFormatted = Html.FromHtml("<b>Sector</b> ", FromHtmlOptions.ModeLegacy);

            confidentialCheckBox.Checked = mEvent.Confidencial;

            ShowEventNotes();
        }

        void OnDocumentClick(object sender, AttachmentFile documentSelected)
        {
            DownloadFileToShowIt(documentSelected);
        }

        private void DownloadFileToShowIt(AttachmentFile documentFile)
        {
            ShowProgress(true);

            Task.Run(async () =>
            {

                try
                {
                    if (documentFile.Id != null)
                    {
                        byte[] bytesArray = await AysaClient.Instance.GetFile(documentFile.Id);

                        RunOnUiThread(() =>
                        {
                            SaveDocumentInTemporaryFolder(documentFile.FileName, bytesArray);
                        });
                    }
                    else
                    {
                        int index = -1;

                        for (int i = 0; i < attachedNoteFiles.Count; i++)
                        {
                            AttachmentFile file = attachedNoteFiles[i];
                            if (file.FileName == documentFile.FileName)
                            {
                                index = i;
                                i = attachedNoteFiles.Count;
                            }
                        }

                        if (index != -1)
                        {
                            var file = attachedNoteFiles[index];
                            SaveDocumentInTemporaryFolder(file.FileName, file.BytesArray);
                        }
                    }
                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        ShowProgress(false);
                    });
                }
            });
        }

        private void SaveDocumentInTemporaryFolder(String nameFile, byte[] bytes)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                // Save Document
                var directory = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                directory = Path.Combine(directory, Android.OS.Environment.DirectoryDownloads);
                string filePath = Path.Combine(directory, nameFile);
                System.IO.File.WriteAllBytes(filePath, bytes);

                // Show document saved
                ShowDocumentSaved(filePath);
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
            }
        }

        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".pdf":
                    return "application/pdf";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return "image/jpeg";
                case ".txt":
                    return "text/plain";
                default:
                    return "*/*";
            }
        }

        private void ShowDocumentSaved(string filePath)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                pdfpath = AndroidX.Core.Content.FileProvider.GetUriForFile(this, this.PackageName + ".fileprovider", new Java.IO.File(filePath));
            }
            else
            {
                pdfpath = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
            }
            Intent intent = new Intent(Intent.ActionView);
            string mimeType = GetMimeType(Path.GetExtension(filePath));
            intent.SetDataAndType(pdfpath, mimeType);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);
            intent.AddFlags(ActivityFlags.NoHistory);
            StartActivity(Intent.CreateChooser(intent, "Eliga una aplicación"));
        }

        private void ShowEventDocuments()
        {
            if (mEvent.Archivos != null)
            {
                LinearLayout documentsContainer = FindViewById<LinearLayout>(Resource.Id.eventsDocumentList);
                documentsContainer.RemoveAllViews();
                foreach (AttachmentFile obs in mEvent.Archivos)
                {
                    string privatePublicc;
                    if(obs.Private)
                    {
                        privatePublicc = "Privado";
                    }
                    else
                    {
                        privatePublicc = "Publico";
                    }
                    LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
                    ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.EventDocumentRow, documentsContainer, false);
                    view.FindViewById<TextView>(Resource.Id.txtDocumentName).Text = obs.FileName;
                    view.FindViewById<TextView>(Resource.Id.txtDocumentName).PaintFlags = Android.Graphics.PaintFlags.UnderlineText;

                    documentsContainer.AddView(view);
                    view.Click += (sender, e) => OnDocumentClick(this, obs);

                }
                eventsDocumentList.Parent.RequestLayout();
            }
        }

        private void GetUserInfo()
        {
            Task.Run(async () =>
            {
                try
                {
                    var userLogged = await AysaClient.Instance.GetUserInfo();

                    this.RunOnUiThread(() =>
                    {
                        UserSession.Instance.Id = userLogged.Id;
                        UserSession.Instance.nomApel = userLogged.NombreApellido;
                        UserSession.Instance.Roles = userLogged.Roles;
                    });
                }
                catch (HttpUnauthorized)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });
        }

        private void ShowEventNotes()
        {
            if (mEvent.Observaciones != null)
            {
                var roles = UserSession.Instance.Roles;

                mEvent.Observaciones = mEvent.Observaciones.OrderByDescending(x => x.Fecha).ToList();

                if (!roles.Any(r => r.Nombre.Equals("Directorio")))
                {
                    privateCheckBox.Visibility = ViewStates.Invisible;
                    privateTag.Visibility = ViewStates.Invisible;
                    mEvent.Observaciones = mEvent.Observaciones.Where(x => !x.Directorio).ToList();
                }

                LinearLayout eventNotesContainer = FindViewById<LinearLayout>(Resource.Id.event_notes_container);
                eventNotesContainer.RemoveAllViews();

                foreach (Observation obs in mEvent.Observaciones)
                {
                    LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
                    ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.EventNotesRow, eventNotesContainer, false);
                    view.FindViewById<TextView>(Resource.Id.txtObservacionCreador).Text = obs.Usuario == null ? "" : "Autor: " + obs.Usuario.NombreApellido;
                    view.FindViewById<TextView>(Resource.Id.textView1).Text = obs.Sector == null ? "-" : obs.Sector.Nombre;
                    view.FindViewById<TextView>(Resource.Id.txtObservacionFecha).Text = obs.Fecha.ToString(AysaConstants.FormatDateTime);
                    view.FindViewById<TextView>(Resource.Id.txtNotes).Text = obs.Observacion;

                    //if the obs have files
                    if(obs.Archivos != null)
                    {
                        LinearLayout FilesNotesLayout = view.FindViewById<LinearLayout>(Resource.Id.FilesContainerObservation);
                        LayoutInflater FilesInflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

                        foreach (AttachmentFile f in obs.Archivos)
                        {
                            ViewGroup DocView = (ViewGroup)FilesInflater.Inflate(Resource.Layout.EventDocumentRow, FilesNotesLayout, false);
                            DocView.FindViewById<TextView>(Resource.Id.txtDocumentName).Text = f.FileName;
                            DocView.FindViewById<TextView>(Resource.Id.txtDocumentName).PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
                            FilesNotesLayout.AddView(DocView);
                            view.Click += (sender, e) => OnDocumentClick(this, f);
                        }
                    }
                    eventNotesContainer.AddView(view);
                }
            }
        }

        private void SetUpViewAccordingUserPermissions()
        {

            if (UserSession.Instance.CheckIfUserHasPermission(Model.Permissions.ModificarEvento))
            {
                // Allow to user do everything
                // Check if user can edit confidential field.
                // The user can edit confidential field if he has guard responsable in section 1 or 2
                if (!UserSession.Instance.CheckIfUserIsGuardResponsableOfMainSection())
                {
                    // The user doesn't have guard responsable in section 1 or 2, he can't edit confidential field
                    HiddenConfidentialField();
                }

                return;
            }
            else
            {
                if (UserSession.Instance.CheckIfUserHasPermission(Model.Permissions.ModificarEventoAutorizado))
                {
                    if (UserSession.Instance.CheckIfUserHasActiveSections())
                    {
                        // User has active section so he can add observations

                        // Check if user can edit confidential field.
                        // The user can edit confidential field if he has guard responsable in section 1 or 2
                        if (!UserSession.Instance.CheckIfUserIsGuardResponsableOfMainSection())
                        {
                            // The user doesn't have guard responsable in section 1 or 2, he can't edit confidential field
                            HiddenConfidentialField();
                        }

                        return;
                    }
                    else
                    {
                        // User doesn't have active sections so he can't add observations
                        // The user only can edit the events that they were created by himself
                        
                        HiddenAddObservationContent();
                        HiddenConfidentialField();
                        return;
                    }
                }
            }
        }

        private void HiddenConfidentialField()
        {
            View confidentialContent = FindViewById<View>(Resource.Id.confidentialContent);
            confidentialContent.Visibility = ViewStates.Gone;
        }

        private void HiddenAddObservationContent()
        {
            View addObservationContent = FindViewById<View>(Resource.Id.addObservationContent);
            addObservationContent.Visibility = ViewStates.Gone;
        }

        private void ShowProgress(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void ShowSessionExpiredError()
        {
            //global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            alert.SetMessage("Su sesión ha expirado");
            alert.SetButton("Aceptar", (c, ev) =>
            {
                Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
                StartActivity(shortDialDetails);
                this.Finish();
            });

            alert.Show();
        }

        private void ShowErrorAlert(string message)
        {
            global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            if (!IsFinishing)
            {
                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.evdetails_menu, menu);
            menu.GetItem(0).SetShowAsAction(ShowAsAction.Always);

            // Disable edit button in case that the user doesn't have permissions 
            menu.GetItem(0).SetVisible(showEditButton);


            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
        
            if (item.ItemId == Resource.Id.action_edit_event)
            {
                if (clickEditButton)
                {
                    var editEventActivity = new Intent(this, typeof(AddEventActivity));
                    StartActivityForResult(editEventActivity, EDIT_EVENT_ACTIVITY_CODE);

                    return true;
                }
                
            }
            return base.OnOptionsItemSelected(item);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == EDIT_EVENT_ACTIVITY_CODE)
            {
                if (resultCode.Equals(Result.Ok))
                {
                    editedEvent = true;
                    LoadFullEvent(this.mEvent);
                }
            }
        }

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }

        public override void OnBackPressed()
        {
            if (editedEvent)
            {
                SetResult(Result.Ok);
                Finish();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        private void CloseAction(object sender, DialogClickEventArgs e)
        {
            return;
        }
    }
}