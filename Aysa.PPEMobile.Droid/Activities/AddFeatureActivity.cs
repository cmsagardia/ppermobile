using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Aysa.PPEMobile.Droid.Fragments;
using Aysa.PPEMobile.Droid.Utilities;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "AddFeatureActivity")]
    public class AddFeatureActivity : AppCompatActivity
    {
        public static readonly int PickFileId = 1000;
        private List<Roles> roles = UserSession.Instance.Roles;
        private Semana semana;
        private SemanaSeccion semanaxSeccion;
        private string oldSectionSelected;
        private string newSectionSelected;


        FrameLayout progressOverlay;

        // Private Variables
        List<Section> ActiveSectionsList;
        Section Sectors;

        EditText dateEditText;
        EditText detailEditText;
        EditText authorEditText;
        Spinner spinnerSectors;
        LinearLayout filesContainer;
        TextView description_note_1;
        TextView list_note;
        TextView description_note_2;
        bool openTimePicker = false;
        int limitDetalle = 1000;

        Button btnSegmented1;
        Button btnSegmented2;

        List<AttachmentFile> attachedFiles = new List<AttachmentFile>();
        List<AttachmentFile> uploadedAttachmentFiles = new List<AttachmentFile>();
        List<AttachmentFile> filesToDeleteInServer = new List<AttachmentFile>();
        int countAttachmentsUploaded = 0;
        int countAttachmentsDeleted = 0;

        private Feature mFeature;

        private bool editMode = false;
        Android.Net.Uri pdfpath;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.mFeature = FeatureDataHolder.getInstance().getData();
            GetActiveWeek();

            SetContentView(Resource.Layout.AddFeature);
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            Button btnCreate = FindViewById<Button>(Resource.Id.btnEventNewsCreate);
            btnCreate.Click += BtnCreate_Click;

            description_note_1 = FindViewById<TextView>(Resource.Id.description_note_1);
            list_note = FindViewById<TextView>(Resource.Id.list_note);
            description_note_2 = FindViewById<TextView>(Resource.Id.description_note_2);

            description_note_1.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString(Resource.String.description_note_1), Android.Text.FromHtmlOptions.ModeLegacy);
            list_note.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString(Resource.String.list_note), Android.Text.FromHtmlOptions.ModeLegacy);
            description_note_2.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString(Resource.String.description_note_2), Android.Text.FromHtmlOptions.ModeLegacy);

            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);

            if (mFeature != null)
            {
                //MODO EDITAR NOVEDAD
                toolbar.Title = "Editar Novedad #" + mFeature.Detail;
                btnCreate.Text = "Guardar";
                editMode = true;
            }
            else
            {
                //MODO AGREGAR NUEVO NOVEDAD
                toolbar.SetTitle(Resource.String.add_feature_title);
                attachedFiles = new List<AttachmentFile>();
            }

            // Add back button
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);


            dateEditText = FindViewById<EditText>(Resource.Id.editTextEventDate);
            detailEditText = FindViewById<EditText>(Resource.Id.editTextEventDetail);
            filesContainer = FindViewById<LinearLayout>(Resource.Id.addEventFilesContainer);
            authorEditText = FindViewById<EditText>(Resource.Id.editTextAuthor);

            User usuario = GetUserLogged();
            authorEditText.Text = usuario.NombreApellido;

            FindViewById<Button>(Resource.Id.btnUploadFiles).Click += UploadFile_Click;

            detailEditText.AfterTextChanged += DetailText_Changed;

            if (mFeature != null)
            {
                dateEditText.Text = mFeature.Date.ToString(AysaConstants.FormatDateTime);
                detailEditText.Text = mFeature.Detail;

                if (mFeature.Archivos != null)
                {
                    foreach (AttachmentFile file in mFeature.Archivos)
                    {
                        addAttachedFile(file);
                    }
                }

                attachedFiles.AddRange(mFeature.Archivos);
			}
			else
			{
                dateEditText.Text = DateTime.Now.ToString(AysaConstants.FormatDateTime);
			}

            // Date Field
            dateEditText.Click += DateEditText_Click;
            dateEditText.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                if (openTimePicker)
                {
                    TimePickerFragment frag = TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        openTimePicker = false;
                        dateEditText.Text += " " + time.ToShortTimeString();
                    });
                    frag.Show(SupportFragmentManager, TimePickerFragment.TAG);
                }
            };

            // Sector field
            spinnerSectors = FindViewById<Spinner>(Resource.Id.spinnerSectors);

            LoadActiveSectionsInView();
            
        }

        private async void GetActiveWeek()
        {
            semana = await AysaClient.Instance.GetActiveWeek();
        }

        private async void UploadFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                {
                    if(!ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage))
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
                           "application/rar",
                           "image/*",
                           "video/*",
                           "text/plain"
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
                            fis = new FileInputStream(new File(pickResult.FullPath));

                            byte[] buf = new byte[1024];
                            int n;
                            while (-1 != (n = fis.Read(buf)))
                                baos.Write(buf, 0, n);

                            Byte[] fileByteArray = baos.ToByteArray();
                            file.BytesArray = fileByteArray;

                            attachedFiles.Add(file);
                            addAttachedFile(file);
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

            //old code
            //if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            //{
            //    String[] mimeTypes =
            //        {"application/msword","application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .doc & .docx
            //        "application/vnd.ms-PowerPoint","application/vnd.openxmlformats-officedocument.presentationml.presentation", // .ppt & .pptx
            //        "application/vnd.ms-Excel","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xls & .xlsx
            //        "text/plain",
            //        "application/pdf",
            //        "application/Zip",
            //        "image/*"
            //        };

            //    Intent intent = new Intent(Intent.ActionGetContent);
            //    intent.AddCategory(Intent.CategoryOpenable);

            //    if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            //    {
            //        intent.SetType(mimeTypes.Length == 1 ? mimeTypes[0] : "*/*");
            //        if (mimeTypes.Length > 0)
            //        {
            //            intent.PutExtra(Intent.ExtraMimeTypes, mimeTypes);
            //        }
            //    }
            //    else
            //    {
            //        String mimeTypesStr = "";
            //        foreach (String mimeType in mimeTypes)
            //        {
            //            mimeTypesStr += mimeType + "|";
            //        }
            //        intent.SetType(mimeTypesStr.Substring(0, mimeTypesStr.Length - 1));
            //    }
            //    StartActivityForResult(Intent.CreateChooser(intent, "ChooseFile"), PickFileId);
            //    //StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
            //}
            //else
            //{
            //    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
            //}
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickFileId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                string filename = GetFileName(uri);
                AttachmentFile file = new AttachmentFile(filename);

                Byte[] fileByteArray = ConvertUriToByteArray(uri, filename);
                file.BytesArray = fileByteArray;

                attachedFiles.Add(file);

                addAttachedFile(file);
            }
        }

        private byte[] ConvertUriToByteArray(Android.Net.Uri data, string name)
        {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            FileInputStream fis;
            string path = "";
            try
            {
                if (isMediaDocument(data))
                {
                    String docId = DocumentsContract.GetDocumentId(data);
                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];
                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                        split[1]
                    };

                    path = getDataColumn(this, contentUri, selection, selectionArgs);
                }
                else //if is a .docs or .pdf or .xls
                {
                    path = data.LastPathSegment.Split("raw:")[1];
                }
                
                fis = new FileInputStream(new File(path));

                byte[] buf = new byte[1024];
                int n;
                while (-1 != (n = fis.Read(buf)))
                    baos.Write(buf, 0, n);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in convert Byte Array" + e.ToString());

                return null;
            }

            byte[] bbytes = baos.ToByteArray();

            return bbytes;
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }
        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            String column = "_data";
            String[] projection =
            {
                column
            };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        private string GetFileName(Android.Net.Uri uri)
        {
            //string path = GetPath(uri);
            string[] parts = uri.LastPathSegment.Split('/');

            return parts[parts.Length - 1];
        }

        //DEPRECRATED.... we never understood the fu* cursors
        //private string GetPath(Android.Net.Uri uri)
        //{
        //    string path = null;
        //    // The projection contains the columns we want to return in our query.
        //    string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
        //    using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
        //    {
        //        if (cursor != null)
        //        {
        //            int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
        //            cursor.MoveToFirst();
        //            path = cursor.GetString(columnIndex);
        //        }
        //    }

        //    if (string.IsNullOrEmpty(path))
        //    {
        //        ICursor cursor = this.ContentResolver.Query(uri, null, null, null, null);
        //        cursor.MoveToFirst();
        //        string document_id = cursor.GetString(0);

        //        if (document_id.Length > 1)
        //            document_id = document_id.Split(':')[1];

        //        //if document_id already have the document path
        //        var expression = new Regex("/\\w+/.*");
        //        if (expression.IsMatch(document_id))
        //            return document_id;

        //        cursor.Close();

        //        cursor = ContentResolver.Query(
        //        Android.Provider.MediaStore.Images.Media.ExternalContentUri,
        //        null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new String[] { document_id }, null);
        //        cursor.MoveToFirst();
        //        path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
        //        cursor.Close();
        //    }
        //    return path;
        //}

        //private string GetPath(Android.Net.Uri uri)
        //{
        //    ICursor cursor = this.ContentResolver.Query(uri, null, null, null, null);
        //    cursor.MoveToFirst();
        //    string document_id = cursor.GetString(0);

        //    if (document_id.Length > 1)
        //        document_id = document_id.Split(':')[1];

        //    cursor.Close();

        //    cursor = ContentResolver.Query(
        //    Android.Provider.MediaStore.Images.Media.ExternalContentUri,
        //    null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new String[] { document_id }, null);
        //    cursor.MoveToFirst();
        //    string path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
        //    cursor.Close();

        //    return path;
        //}

        private void addAttachedFile(AttachmentFile file)
        {

            LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
            ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.AddEventDocumentRow, filesContainer, false);
            view.FindViewById<TextView>(Resource.Id.txtDocumentName).Text = file.FileName;
            view.FindViewById<TextView>(Resource.Id.txtDocumentName).PaintFlags = Android.Graphics.PaintFlags.UnderlineText;
            view.FindViewById<ImageView>(Resource.Id.btnRemoveEventDocument).Click += ClickRemoveFile;

            filesContainer.AddView(view);

            view.Click += (sender, e) => OnDocumentClick(this, file);
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

                        for (int i = 0; i < attachedFiles.Count; i++)
                        {
                            AttachmentFile file = attachedFiles[i];
                            if (file.FileName == documentFile.FileName)
                            {
                                index = i;
                                i = attachedFiles.Count;
                            }
                        }

                        if (index != -1)
                        {
                            var file = attachedFiles[index];
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
                directory = System.IO.Path.Combine(directory, Android.OS.Environment.DirectoryDownloads);
                string filePath = System.IO.Path.Combine(directory, nameFile);
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
            string mimeType = GetMimeType(System.IO.Path.GetExtension(filePath));
            intent.SetDataAndType(pdfpath, mimeType);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);
            intent.AddFlags(ActivityFlags.NoHistory);
            StartActivity(Intent.CreateChooser(intent, "Eliga una aplicación"));
        }

        private void ClickRemoveFile(object sender, EventArgs e)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            alert.SetMessage("¿Está seguro que desea quitar el Archivo?");
            alert.SetButton("Si", (c, ev) =>
            {
                ViewGroup fileRow = (ViewGroup)((View)sender).Parent;
                int filePosition = filesContainer.IndexOfChild(fileRow);

                DeleteFile(attachedFiles[filePosition], filePosition);

            });

            alert.SetButton2("Cancelar", (c, ev) => { });

            alert.Show();


        }

        private void DeleteFile(AttachmentFile documentFile, int filePosition)
        {

            // Delete file from server if it's necessary 
            // If the file has id that means that it was upload to the server, so it's needed to remove
            if (documentFile.Id != null)
            {
                filesToDeleteInServer.Add(documentFile);

                // Call WS to remove file in server
                //RemoveFileFromServer(documentFile, filePosition);
            }

            // Remove file in view
            attachedFiles.RemoveAt(filePosition);

            filesContainer.RemoveViewAt(filePosition);

        }


        private void RemoveFilesFromServer()
        {

            //Show Progress
            ShowProgressDialog(true);

            Task.Run(async () =>
            {

                try
                {

                    AttachmentFile fileDelete = filesToDeleteInServer[countAttachmentsDeleted];

                    AttachmentFile response = await AysaClient.Instance.DeleteFile(fileDelete.Id);

                    RunOnUiThread(() =>
                    {
                        countAttachmentsDeleted++;

                        if (filesToDeleteInServer.Count() > countAttachmentsDeleted)
                        {
                            RemoveFilesFromServer();
                        }
                        else
                        {

                            // Finish to delete files, continue with the secuence
                            if (attachedFiles.Count > 0)
                            {
                                UploadFilesToServer();
                            }
                            else
                            {
                                UploadFeatureToServer();

                            }
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
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        // Remove progress
                        ShowProgressDialog(false);
                    });

                }
            });

        }

        //private void RemoveFileFromServer(AttachmentFile file)
        //{

        ////Show Progress
        //ShowProgressDialog(true);

        //Task.Run(async () =>
        //{

        //try
        //{
        //    string response = await AysaClient.Instance.DeleteFile(file.Id);

        //    RunOnUiThread(() =>
        //    {
        //        global::Android.Support.V7.App.AlertDialog.Builder builder = new global::Android.Support.V7.App.AlertDialog.Builder(this);
        //        builder.SetTitle("Aviso");
        //        builder.SetMessage("El archivo ha sido eliminado");
        //        builder.SetPositiveButton("OK", OkAction);

        //        Dialog dialog = builder.Create();
        //        dialog.Show();

        //    });

        //}
        //catch (HttpUnauthorized)
        //{
        //RunOnUiThread(() =>
        //{
        //ShowErrorAlert("No tiene permisos para ejecutar esta acción");
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            RunOnUiThread(() =>
        //            {
        //                ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
        //            });
        //        }
        //        finally
        //        {
        //            RunOnUiThread(() =>
        //            {
        //                // Remove progress
        //                ShowProgressDialog(false);
        //            });

        //        }
        //    });
        //}

        private void DateEditText_Click(object sender, EventArgs e)
        {
            
            Task.Run(async () =>
            {
                //Check if an update is needed
                await UpdateDateBySection();

                if (!roles.Any(r => r.Nombre.Equals("Referente PPE")) && roles.Any(r => r.Nombre.Equals("Responsable Guardia")))
                {
                    Fragments.DatePickerRGFragment frag = Fragments.DatePickerRGFragment.NewInstance(delegate (DateTime time)
                    {
                        openTimePicker = true;
                        dateEditText.Text = time.ToString("dd/MM/yyyy");
                    }, semana);
                    frag.Show(SupportFragmentManager, Fragments.DatePickerRGFragment.TAG);
                }
                else {
                    Fragments.DatePickerFragment frag = Fragments.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        openTimePicker = true;
                        dateEditText.Text = time.ToString("dd/MM/yyyy");
                    });
                    frag.Show(SupportFragmentManager, Fragments.DatePickerFragment.TAG);
                }
            });
            
        }

        private void DetailText_Changed(object sender, EventArgs e)
        {
            if (detailEditText.Text.Length == 1000)
            {
                global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
                alert.SetTitle("Aviso");
                alert.SetMessage($"Para el campo Detalle:\nAlcanzó el límite de {limitDetalle} caracterres");
                alert.SetPositiveButton("Ok", CloseAction);

                Dialog dialog = alert.Create();
                dialog.Show();
                return;
            }
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            //Check if an update is needed
            await UpdateDateBySection();

            string validateErrorMessage = ValidateInputFields();
            if (validateErrorMessage.Length > 0)
            {
                // There are errors in the Input fields.. show Alert
                ShowErrorAlert(validateErrorMessage);
                ShowProgressDialog(false);
            }
            else
            {
                Feature eventObj = BuildFeatureWillCreateFromUI();
                if (eventObj.Detail.Length > limitDetalle)
                {
                    global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
                    alert.SetTitle("Aviso");
                    alert.SetMessage($"Para el campo Detalle:\nSupero limite de {limitDetalle} caracterres");
                    alert.SetPositiveButton("Ok", CloseAction);

                    Dialog dialog = alert.Create();
                    dialog.Show();
                    return;
                }
                AttemptFeatureUpload();
            }
        }

        private void AttemptFeatureUpload()
        {

            //Show progress
            ShowProgressDialog(true);


            if (filesToDeleteInServer.Count > 0)
            {
                // Init secuence to remove files
                RemoveFilesFromServer();
            }
            else
            {
                if (attachedFiles.Count > 0)
                {
                    UploadFilesToServer();
                }
                else
                {
                    UploadFeatureToServer();

                }
            }
        }


        private void UploadFeatureToServer()
        {
            Feature eventObj = BuildFeatureWillCreateFromUI();

            if (eventObj != null)
            {
                SendFeatureToServer(eventObj);
            }
        }

        private void UploadFilesToServer()
        {
            // Upload files

            Task.Run(async () =>
            {

                try
                {
                    AttachmentFile fileInMemory = attachedFiles[countAttachmentsUploaded];

                    // If Attachment file doesn't have file that means that it's already added
                    if (fileInMemory.BytesArray == null)
                    {
                        countAttachmentsUploaded++;
                        if (countAttachmentsUploaded < attachedFiles.Count())
                        {
                            UploadFilesToServer();
                        }
                        else
                        {
                            UploadFeatureToServer();
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

                        if (countAttachmentsUploaded < attachedFiles.Count)
                        {

                            UploadFilesToServer();
                        }
                        else
                        {
                            UploadFeatureToServer();
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
                        ShowProgressDialog(false);

                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }

            });

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Finish();
            return base.OnOptionsItemSelected(item);
        }

        private void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private string ValidateInputFields()
        {

            // Validate Fecha de Ocurrencia
            if (IsEditTextEmpty(dateEditText))
            {
                return "El campo Fecha de Ocurrencia no puede estar  vacío";
            }

            if (!roles.Any(r => r.Nombre == "Referente PPE") &&
                roles.Any(r => r.Nombre == "Responsable Guardia"))
            {
                //Validate Date - future
                if (IsInvalidFutureDate(dateEditText))
                {
                    return "No puede cargar novedades con día o horario mayor al actual.";
                }

                //Validate Date - out of guard - past
                if (IsInvalidMinDate(dateEditText))
                {
                    return "No puede cargar novedades con día o horario anterior al inicio de la guardia.";
                }

                //Validate Date - out of guard - future
                if (IsInvalidMaxDate(dateEditText))
                {
                    return "No puede cargar novedades con día o horario posterior al de finalización de la guardia.";
                }

                if (IsPastLimitDate())
                {
                    return "No puede cargar novedades luego de las 17hs una vez finalizada la guardia.";
                }
            }

            // Validate Lugar
            if (IsEditTextEmpty(detailEditText))
            {
                return "El campo Detalle no puede estar  vacío";
            }

            // Validate Section
            if (IsSpinnerEmpty(spinnerSectors))
            {
                return "Es necesario seleccionar un Sector";
            }

            return "";
        }



        private bool IsSpinnerEmpty(Spinner spinner)
        {
            return spinner.SelectedItem == null;
        }

        private bool IsEditTextEmpty(EditText editText)
        {
            return editText.Text == null || editText.Text.Trim().Count() == 0;
        }

        private bool IsInvalidFutureDate(EditText date)
        {
            DateTime dateSelected = DateTime.ParseExact(date.Text, AysaConstants.FormatDateTime, null);

            DateTime currently = DateTime.Now;

            bool isInvalid = dateSelected > currently;

            return isInvalid;
        }

        private bool IsInvalidMinDate(EditText date)
        {
            DateTime dateSelected = DateTime.ParseExact(date.Text, AysaConstants.FormatDateTime, null);

            bool isInvalid = dateSelected < semana.Desde;

            return isInvalid;
        }

        private bool IsInvalidMaxDate(EditText date)
        {
            DateTime dateSelected = DateTime.ParseExact(date.Text, AysaConstants.FormatDateTime, null);

            bool isInvalid = dateSelected > semana.Hasta;

            return isInvalid;
        }

        private bool IsPastLimitDate()
        {
            DateTime currently = DateTime.Now;

            bool isInvalid = currently > semana.Hasta && currently.Hour >= 17;

            return isInvalid;
        }

        private bool IsEmptyTextView(AppCompatEditText editText)
        {
            if (editText.Text.Length == 0)
            {
                ColorStateList colorStateList = ColorStateList.ValueOf(Color.Red);
                editText.SupportBackgroundTintList = colorStateList;
                return true;
            }
            else
            {
                ColorStateList colorStateList = ColorStateList.ValueOf(Color.Gray);
                editText.SupportBackgroundTintList = colorStateList;

                return false;
            }
        }

        private User GetUserLogged()
        {
            User user = new User();
            user.UserName = UserSession.Instance.UserName;
            user.Id = UserSession.Instance.Id;
            user.NombreApellido = UserSession.Instance.nomApel;
            
            return user;
        }


        //public void GetEventTypesFromServer()
        //{
        //    // Get events type from server

        //    Task.Run(async () =>
        //    {

        //        try
        //        {
        //            EventTypesList = await AysaClient.Instance.GetEventsType();

        //            RunOnUiThread(() =>
        //            {
        //                // Fill spinner items
        //                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, EventTypesList.ToArray());
        //                spinnerType.Adapter = adapter;

        //                if (EventTypesList != null && mFeature != null)
        //                {
        //                    for (int i = 0; i < EventTypesList.Count(); i++)
        //                    {
        //                        EventType type = EventTypesList[i];
        //                        if (type.Id == "1")
        //                        {
        //                            spinnerType.SetSelection(i);
        //                            break;
        //                        }
        //                    }
        //                }
        //            });

        //        }
        //        catch (HttpUnauthorized)
        //        {
        //            RunOnUiThread(() =>
        //            {
        //                ShowErrorAlert("Sesión expirada.");
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            RunOnUiThread(() =>
        //            {
        //                ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
        //            });
        //        }
        //        finally
        //        {
        //        }
        //    });

        //}

        public async Task UpdateDateBySection()
        {
            newSectionSelected = ActiveSectionsList[spinnerSectors.SelectedItemPosition].Id;
            //If need update Date?
            if (oldSectionSelected != newSectionSelected)
            {
                await GetDateBySection();
                oldSectionSelected = newSectionSelected;
            }
        }

        public async Task GetDateBySection()
        {
                try
                {
                    semanaxSeccion = await AysaClient.Instance.SearchDateBySection(ActiveSectionsList[spinnerSectors.SelectedItemPosition].Id);
                    semana.Desde = semanaxSeccion.Desde;
                    semana.Hasta = semanaxSeccion.Hasta;
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
                }
        }

        public void LoadActiveSectionsInView()
        {
            ActiveSectionsList = UserSession.Instance.ActiveSections;
            List<Section> SectorGuardList = UserSession.Instance.PersonInGuard.Sectores;

            // After get Sections Active list, load elements in PickerView
            if (ActiveSectionsList != null && ActiveSectionsList.Count > 0)
            {
                spinnerSectors.Enabled = true;

                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, ActiveSectionsList.ToArray());
                spinnerSectors.Adapter = adapter;

                if (mFeature == null)//adding a new feature
                {
                    for (int i = 0; i < ActiveSectionsList.Count(); i++)
                    {
                        for (int j = 0; j < SectorGuardList.Count(); j++)
                        {
                            if (ActiveSectionsList[i].Nombre.Equals(SectorGuardList[j].Nombre))
                            {
                                spinnerSectors.SetSelection(i);
                                break;
                            }
                        }
                    }
                }
                else //editing feature
                {
                    for (int i = 0; i < ActiveSectionsList.Count(); i++)
                    {
                        if (ActiveSectionsList[i].Id == mFeature.Sector.Id)
                        {
                            spinnerSectors.SetSelection(i);
                            break;
                        }
                    }
                }
                //if (ActiveSectionsList != null && mFeature != null)
                //{
                //    for (int i = 0; i < ActiveSectionsList.Count(); i++)
                //    {
                //        Section type = ActiveSectionsList[i];
                //        if (type.Id == mFeature.Sector.Id)
                //        {
                //            spinnerSectors.SetSelection(i);
                //            break;
                //        }
                //    }
                //}
            }
            else
            {
                spinnerSectors.Enabled = false;

                List<Section> auxSections = new List<Section>();
                auxSections.Add(mFeature.Sector);
                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, auxSections.ToArray());
                spinnerSectors.Adapter = adapter;
            }
        }

        private Feature BuildFeatureWillCreateFromUI()
        {
            // Create Feature
            Feature eventObj = new Feature();
            eventObj.Id = mFeature != null ? mFeature.Id : null;
            eventObj.Date = DateTime.ParseExact(dateEditText.Text, AysaConstants.FormatDateTime, null);
            eventObj.Detail = detailEditText.Text;

            if (ActiveSectionsList == null || ActiveSectionsList.Count() <= 0)
            {
                eventObj.Sector = Sectors;
            }
            else
            {
                Section auxSector = ActiveSectionsList[spinnerSectors.SelectedItemPosition];
                eventObj.Sector = auxSector;
            }

            eventObj.Usuario = GetUserLogged();


            if (mFeature != null)
            {
                eventObj.Archivos = mFeature.Archivos;
                // Concatenate new files
                if(filesToDeleteInServer.Count() > 0)
                {
                    foreach (AttachmentFile file in filesToDeleteInServer)
                    {
                        eventObj.Archivos.Remove(file);
                    }
                }

                if (uploadedAttachmentFiles.Count() > 0)
                {
                    eventObj.Archivos.AddRange(uploadedAttachmentFiles);
                }

            }
            else
            {
                eventObj.Archivos = uploadedAttachmentFiles;
            }

            return eventObj;
        }

        private void SendFeatureToServer(Feature eventObj)
        {

            Task.Run(async () =>
            {

                try
                {
                    Feature featureCreated;

                    if (mFeature == null)
                    {
                        //Create Feature
                        featureCreated = await AysaClient.Instance.CreateFeature(eventObj);
                    }
                    else
                    {
                        // Update Feature
                        featureCreated = await AysaClient.Instance.UpdateFeature(mFeature.Id, eventObj);
                        FeatureDataHolder.getInstance().setData(featureCreated);
                    }

                    RunOnUiThread(() =>
                    {
                        OnFeatureCreated();
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
                        ShowProgressDialog(false);

                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });
        }

        private void ShowErrorAlert(string message)
        {
            global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void ShowSessionExpiredError()
        {
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

        private void OnFeatureCreated()
        {
            global::AndroidX.AppCompat.App.AlertDialog.Builder builder = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("Aviso");
            builder.SetMessage(editMode ? "La novedad ha sido editada con éxito" : "La novedad ha sido creada con éxito");
            builder.SetPositiveButton("OK", OkAction);
            builder.SetCancelable(false);

            Dialog dialog = builder.Create();
            dialog.Show();
        }

        private void OnFeatureModified()
        {
            global::AndroidX.AppCompat.App.AlertDialog.Builder builder = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("Aviso");
            builder.SetMessage("No puedes modificar una novedad de la que no eres autor.");
            builder.SetPositiveButton("OK", OkAction);
            builder.SetCancelable(false);

            Dialog dialog = builder.Create();
            dialog.Show();
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            SetResult(Result.Ok);
            Finish();
        }

        private void CloseAction(object sender, DialogClickEventArgs e)
        {
            return;
        }

        private void ShowProgress(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }
    }
}