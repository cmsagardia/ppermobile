
using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Aysa.PPEMobile.Model;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.Droid.Activities;
using Newtonsoft.Json;
using System.IO;
using Android;
using Android.Content.PM;
using AndroidX.RecyclerView.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.App;

namespace Aysa.PPEMobile.Droid.Fragments
{
    public class DocumentsFragment : global::AndroidX.Fragment.App.Fragment
    {

        // Save Documents
        private static readonly String DOCUMENT_SAVED_KEY = "DOCUMENT_SAVED_LIST";
        private static readonly String DOCUMENT_PREFERENCES_SAVED_KEY = "PREFERENCE_DOCUMENTS";

        FrameLayout progressOverlay;

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        DocumentListAdapter mAdapter;
        List<Document> DocumentsList;
        Android.Net.Uri pdfpath;

        bool isOffline = false;

        public static DocumentsFragment newInstance()
        {
            Bundle args = new Bundle();
            DocumentsFragment fragment = new DocumentsFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("DocumentsFragment.OnCreateView");
            View view = inflater.Inflate(Resource.Layout.DocumentsFragment, null);

            SetUpView(view);

            if (this.Activity.Intent.GetStringExtra("offlineMode") != null)
            {
                System.Diagnostics.Debug.WriteLine("    DocumentsFragment.OfflineMode");
                isOffline = true;
                // Show documents in offline mode
                GetDocumentsSavedInDevice();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("    DocumentsFragment.DocumentsFromServer");
                GetDocumentsFromServer();
            }

            return view;
        }

        #region Private Methods

        private void SetUpView(View view)
        {
            progressOverlay = view.FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            InitTableView(view);
        }

        private void InitTableView(View view)
        {
            System.Diagnostics.Debug.WriteLine("    DocumentsFragment.InitTableView");

            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mLayoutManager = new LinearLayoutManager(this.Activity);
            mRecyclerView.SetLayoutManager(mLayoutManager);
        }

        private void ShowSessionExpiredError()
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this.Activity);
            AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            alert.SetMessage("Su sesión ha expirado");
            alert.SetButton("Aceptar", (c, ev) =>
            {
                Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
                StartActivity(shortDialDetails);
                this.Activity.Finish();
            });

            alert.Show();
        }

        //private void ShowSessionExpiredError()
        //{
        //    //TODO
        //    Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
        //    StartActivity(shortDialDetails);
        //    this.Activity.Finish();
        //}

        private void ShowErrorAlert(string message)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this.Activity);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void ShowProgressDialog(bool show)
        {
            System.Diagnostics.Debug.WriteLine("    DocumentsFragment.ShowProgressDialog " +show);
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
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

        private void SaveDocumentInTemporaryFolder(String nameFile, byte[] bytes)
        {
            if(ContextCompat.CheckSelfPermission(this.Activity, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
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
                ActivityCompat.RequestPermissions(this.Activity, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
            }
            
        }

        private void ShowDocumentSaved(string filePath)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                pdfpath = FileProvider.GetUriForFile(this.Context, this.Context.PackageName + ".fileprovider", new Java.IO.File(filePath));
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

        private void GetDocumentsSavedInDevice()
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences(DOCUMENT_PREFERENCES_SAVED_KEY, FileCreationMode.Private);

            // read exisiting value
            var documents = pref.GetString(DOCUMENT_SAVED_KEY, null);

            if (documents == null)
            {
                this.DocumentsList = new List<Document>();
            }
            else
            {
                this.DocumentsList = JsonConvert.DeserializeObject<List<Document>>(documents);
            }

            setAdapter();
        }

        private void setAdapter()
        {
            System.Diagnostics.Debug.WriteLine("    DocumentsFragment.setAdapter "+this.DocumentsList.Count());
            mAdapter = new DocumentListAdapter(this.DocumentsList);

            mAdapter.ItemClick += OnItemClick;

            mRecyclerView.SetAdapter(mAdapter);
            mAdapter.NotifyDataSetChanged();

            if (mAdapter.ItemCount > 0)
            {
                mRecyclerView.SmoothScrollToPosition(0);
            }
            mRecyclerView.ForceLayout();
        }




        private void GetDocumentsFromServer()
        {
			// Get documents from server
			ShowProgressDialog(true);

			Task.Run(async () =>
			{

				try
				{
					this.DocumentsList = await AysaClient.Instance.GetDocuments();

					this.Activity.RunOnUiThread(() =>
					{
						setAdapter();
					});
				}
				catch (HttpUnauthorized)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowSessionExpiredError();
					});
				}
				catch (NetworkConnectionException)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
					});
				}
				catch (Exception ex)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert($"No se pudo descargar la lista de Documentos");
					});
				}
				finally
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowProgressDialog(false);
					});
				}
			});

		}


        private void GetDocumentOnlineMode(Document doc)
        {
            //Show a progress layout
            ShowProgressDialog(true);

            // Get Document From Server

            Task.Run(async () =>
            {

                try
                {
                    // Download file from server
                    byte[] bytesArray = await AysaClient.Instance.GetDocumentFile(doc.ServerRelativeUrl);

                    // Encode Data
                    var text = System.Text.Encoding.Default.GetString(bytesArray);
                    text = text.Replace("\"", "");
                    bytesArray = Convert.FromBase64String(text);
                    String name = "Documentos Offline" + " " + doc.Name;

                    this.Activity.RunOnUiThread(() =>
                    {
                        // Save and show document downloaded
                        SaveDocumentInTemporaryFolder(name, bytesArray);
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Remove progress layout
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowProgressDialog(false);
                    });
                }

            });
        }

        // Handler for the item click event:
        void OnItemClick(object sender, Document documentSelected)
        {
            if(isOffline)
            {
                // Save and show document downloaded
                SaveDocumentInTemporaryFolder(documentSelected.Name, documentSelected.BytesArray);
            }
            else
            {
                GetDocumentOnlineMode(documentSelected);
            }
        }

        #endregion

    }


    // Implement the ViewHolder pattern: each ViewHolder holds references
    // to the UI components (ImageView and TextView) within the CardView 
    // that is displayed in a row of the RecyclerView:
    public class DocumentViewHolder : RecyclerView.ViewHolder
    {
        //public ImageView Image { get; private set; }
        public TextView txtDocumentTitle { get; private set; }

        // Get references to the views defined in the CardView layout.
        public DocumentViewHolder(View itemView): base(itemView)
        {
            txtDocumentTitle = itemView.FindViewById<TextView>(Resource.Id.txt_title);
        }

    }

    // ADAPTER
    public class DocumentListAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<Document> ItemClick;

        // Underlying data set (a photo album):
        public List<Document> mDocumentsList;

        // Load the adapter with the data set (photo album) at construction time:
        public DocumentListAdapter(List<Document> documents)
        {
            mDocumentsList = documents;
        }

        // Create a new photo CardView (invoked by the layout manager): 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.DocumentRow, parent, false);

            // Create a ViewHolder to find and hold these view references, and 
            // register OnClick with the view holder:
            DocumentViewHolder vh = new DocumentViewHolder(itemView);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            DocumentViewHolder vh = holder as DocumentViewHolder;

            Document doc = mDocumentsList[position];
            vh.txtDocumentTitle.Text = doc.Name;
            vh.ItemView.Click += (sender, e) => OnClick(doc);
        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return mDocumentsList.Count; }
        }

        // Raise an event when the item-click takes place:
        void OnClick(Document document)
        {
            if (ItemClick != null)
                ItemClick(this, document);
        }
    }
}
