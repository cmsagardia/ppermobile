using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Aysa.PPEMobile.Droid.Utilities;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using AndroidX.Core.Content;
using AndroidX.Core.App;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "FeatureDetailActivity")]
    public class FeatureDetailActivity : AppCompatActivity
    {
        FrameLayout progressOverlay;
        private Feature mFeature;
        private TextView tvDetailDate;
        private TextView tvDetailDetail;
        private TextView tvDetailAuthor;
        private LinearLayout featuresDocumentList;
        Spinner spinnerSectors;
        List<Section> ActiveSectionsList;
        readonly int EDIT_FEATURE_ACTIVITY_CODE = 90;
        Android.Net.Uri pdfpath;


        bool showEditButton;
        bool clickEditButton = false;

        //used if this feature was edited through AddEventActivity
        private bool editedFeature = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.mFeature = FeatureDataHolder.getInstance().getData();
            showEditButton = mFeature.CanEdit;

            setUpViews();

            //LoadActiveSectionsInView();

            loadFullFeature(this.mFeature);
        }

        private void setUpViews()
        {
            SetContentView(Resource.Layout.FeatureDetail);

            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            // Set toolbar and title
            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.Title = "Novedad #" + mFeature.Detail;

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            tvDetailDate = FindViewById<TextView>(Resource.Id.textViewEvDetailDate);
            tvDetailDetail = FindViewById<TextView>(Resource.Id.textViewEvDetailDetalle);
            tvDetailAuthor = FindViewById<TextView>(Resource.Id.textViewEvDetailAuthor);
            featuresDocumentList = FindViewById<LinearLayout>(Resource.Id.eventsDocumentList);


            // Sector field
            spinnerSectors = FindViewById<Spinner>(Resource.Id.spinnerSectors);


        }



        //SECTOR
        public void LoadActiveSectionsInView()
        {
            List<Section> featureSectorName = new List<Section>();
            featureSectorName.Add(mFeature.Sector);
            ActiveSectionsList = featureSectorName;

            if (ActiveSectionsList != null && ActiveSectionsList.Count > 0)
            {
                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, ActiveSectionsList.ToArray());
                spinnerSectors.Adapter = adapter;
                spinnerSectors.Enabled = false;
            }
            else
            {
                FindViewById(Resource.Id.spinnerSectorContainer).Visibility = ViewStates.Gone;
            }
        }



        //private void ShowSessionExpiredError()
        //{
        //    global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
        //    alert.SetTitle("Aviso");
        //    alert.SetMessage("Su sesión ha expirado, por favor ingrese sus credenciales nuevamente");

        //    Dialog dialog = alert.Create();
        //    dialog.Show();
        //}

        public async void loadFullFeature(Feature featureSelected)
        {
            ShowProgress(true);
            // Get complete data of feature selected from server
            await Task.Run(async () =>
            {
                try
                {
                    mFeature = await AysaClient.Instance.GetFeatureById(featureSelected.Id);
                    mFeature.Archivos = await AysaClient.Instance.GetFilesOfEvent(mFeature.Id);
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

            showFeatureData();

            mFeature.Archivos = await AysaClient.Instance.GetFilesOfFeature(mFeature.Id);

            FeatureDataHolder.getInstance().setData(mFeature);
            if (mFeature.Archivos.Count > 0)
            {
                showFeatureDocuments();
            }

            ShowProgress(false);
            clickEditButton = true;
        }

        public void showFeatureData()
        {

            String sourceString1 = "<b>Fecha de ocurrencia:</b> " + mFeature.Date.ToString(AysaConstants.FormatDateTime);
            tvDetailDate.TextFormatted = Html.FromHtml(sourceString1, FromHtmlOptions.ModeLegacy);

            String sourceString4 = "<b>Detalle:</b> " + mFeature.Detail;
            tvDetailDetail.TextFormatted = Html.FromHtml(sourceString4, FromHtmlOptions.ModeLegacy);

            String sourceString2 = "<b>Autor:</b> " + mFeature.Usuario.NombreApellido;
            tvDetailAuthor.TextFormatted = Html.FromHtml(sourceString2, FromHtmlOptions.ModeLegacy);


            TextView sectionTextView = FindViewById<TextView>(Resource.Id.section_textView);
            String sourceString3= "<b>Sector:</b> " + mFeature.Sector.Nombre;
            sectionTextView.TextFormatted = Html.FromHtml(sourceString3, FromHtmlOptions.ModeLegacy);

            showFeatureDocuments();

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
                    byte[] bytesArray = await AysaClient.Instance.GetFile(documentFile.Id);

                    //var text = System.Text.Encoding.Default.GetString(bytesArray);
                    //text = text.Replace("\"", "");
                    //bytesArray = Convert.FromBase64String(text);

                    RunOnUiThread(() =>
                    {
                        SaveDocumentInTemporaryFolder(documentFile.FileName, bytesArray);
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
                string filePath = Path.Combine(directory.ToString(), nameFile);
                File.WriteAllBytes(filePath, bytes);

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
                pdfpath = FileProvider.GetUriForFile(this, this.PackageName + ".fileprovider", new Java.IO.File(filePath));
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

        public void showFeatureDocuments()
        {
            if (mFeature.Archivos != null)
            {
                featuresDocumentList.RemoveAllViews();
                foreach (AttachmentFile obs in mFeature.Archivos)
                {
                    LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
                    ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.EventDocumentRow, featuresDocumentList, false);
                    view.FindViewById<TextView>(Resource.Id.txtDocumentName).Text = obs.FileName;
                    view.FindViewById<TextView>(Resource.Id.txtDocumentName).PaintFlags = Android.Graphics.PaintFlags.UnderlineText;

                    featuresDocumentList.AddView(view);
                    view.Click += (sender, e) => OnDocumentClick(this, obs);
                }
                featuresDocumentList.Parent.RequestLayout();
            }
        }

        private void ShowProgress(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
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


        private void OnFeatureModified()
        {
            global::AndroidX.AppCompat.App.AlertDialog.Builder builder = new global::AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("Aviso");
            builder.SetMessage("No tienes guardias activas.");
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
                    var editFeatureActivity = new Intent(this, typeof(AddFeatureActivity));
                    StartActivityForResult(editFeatureActivity, EDIT_FEATURE_ACTIVITY_CODE);

                    return true;
                }
                else
                {
                    OnFeatureModified();
                }

            }
            return base.OnOptionsItemSelected(item);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == EDIT_FEATURE_ACTIVITY_CODE)
            {
                if (resultCode.Equals(Result.Ok))
                {
                    editedFeature = true;
                    var editFeatureActivity = new Intent(this, typeof(FeatureDetailActivity));
                    StartActivity(editFeatureActivity);
                    Finish();
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
            if (editedFeature)
            {
                SetResult(Result.Ok);
                Finish();
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}