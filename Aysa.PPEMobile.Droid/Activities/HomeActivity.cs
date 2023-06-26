using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
//using Android.Support.Design.Widget;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using Aysa.PPEMobile.Droid.Fragments;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait)]
    public class HomeActivity : AppCompatActivity, TabLayout.IOnTabSelectedListener
    {

        private TabLayout tabLayout;

        FrameLayout progressOverlay;


        List<Document> DocumentsList;
        int DocumentsDownloaded = 0;
        List<Document> DocumentsDownloadedList = new List<Document>();
        List<string> tabTitles;

        // Save Documents
        private static readonly String DOCUMENT_SAVED_KEY = "DOCUMENT_SAVED_LIST";
        private static readonly String DOCUMENT_PREFERENCES_SAVED_KEY = "PREFERENCE_DOCUMENTS";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Home);
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);
            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.SetTitle(Resource.String.app_bartitle);

            tabTitles = GetTabTitles();
            ViewPager viewPager = (ViewPager)FindViewById(Resource.Id.fragment_container);
            PagerAdapter pagerAdapter = new PagerAdapter(SupportFragmentManager, tabTitles);
            viewPager.Adapter = pagerAdapter;
            viewPager.OffscreenPageLimit = 4;

            tabLayout = (TabLayout)FindViewById(Resource.Id.tab_layout);
            tabLayout.SetupWithViewPager(viewPager);

            SetSupportActionBar(toolbar);

            // Save documents in the first time
            //if (DocumentsDownloadedList.Count == 0)
            //{
            //    // Get documents to download their files and save it
            //    GetDocumentsFromServer();
            //}

        }

        private List<string> GetTabTitles()
        {
            List<string> items = new List<string>
            {
                "Guardias"
            };

            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarEventos) || UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarEventosAutorizado))
                items.Add("Eventos");
            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarLibroAbordo))
                items.Add("Novedades");

            items.Add("Documentos");
            return items;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {

            // Change text color in menu
            IMenuItem item = menu.FindItem(Resource.Id.close_session);
            SpannableString s = new SpannableString(item.TitleCondensedFormatted);
            s.SetSpan(new ForegroundColorSpan(Color.ParseColor("#672E8A")), 0, s.Length(), 0);

            item.SetTitle(s);

            bool result = base.OnPrepareOptionsMenu(menu);

            return result;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.close_session:

                    // Close session

                    Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                    Android.App.AlertDialog alert = dialog.Create();
                    alert.SetTitle("Aviso");
                    alert.SetMessage("¿Seguro que desea cerrar la sesión?");
                    alert.SetButton("Si", (c, ev) =>
                    {
                        Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
                        StartActivity(shortDialDetails);
                        Finish();
                    });

                    alert.SetButton2("Cancelar", (c, ev) => { });

                    alert.Show();

                    return true;

                case Resource.Id.filter:
                    return false;
            }

            return base.OnOptionsItemSelected(item);
        }


        public void ReplaceFragment(global::AndroidX.Fragment.App.Fragment fragment)
        {
            global::AndroidX.Fragment.App.FragmentManager fragmentManager = SupportFragmentManager;            
            global::AndroidX.Fragment.App.FragmentTransaction transaction = fragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_container  , fragment);
            transaction.Commit();
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            if (tab.Position == 0)
            {
                ReplaceFragment(new ShortDialFragment());
            }
            else if (tab.Position == 1)
            {
                ReplaceFragment(new EventsFragment());
            }
            else if(tab.Position == 2)
            {
                ReplaceFragment(new FeaturesFragment());
            }
            else
            {
                ReplaceFragment(new DocumentsFragment());
            }
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
        }

        public void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        #region Download async Documents

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
            if (string.IsNullOrEmpty(message))
                return;

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void GetDocumentsFromServer()
        {

            Task.Run(async () =>
            {

                try
                {
                    this.DocumentsList = await AysaClient.Instance.GetDocuments();

                    this.RunOnUiThread(() =>
                    {
                        DownloadDocumentAsync();
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
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }

            });

        }

        private void DownloadDocumentAsync()
        {

            // Download the documents one at the time to save and show in offline mode

            // Display an Activity Indicator in the status bar
            //UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            if (DocumentsDownloaded < DocumentsList.Count)
            {
                Document doc = DocumentsList[DocumentsDownloaded];
                DownloadDocumentFile(doc);
                DocumentsDownloaded++;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Documents download completed");
            }

        }

        private void DownloadDocumentFile(Document doc)
        {

            System.Diagnostics.Debug.WriteLine("Downloading..." + doc.Name);

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

                    this.RunOnUiThread(() =>
                    {
                        // Save in device
                        // Build document with its file to save in the device
                        Document doc1 = new Document();
                        doc1.Name = doc.Name;
                        doc1.ServerRelativeUrl = doc.ServerRelativeUrl;
                        doc1.BytesArray = bytesArray;


                        SaveDocumentInDevice(doc1);

                        DownloadDocumentAsync();

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
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });
        }

        private void SaveDocumentInDevice(Document doc)
        {

            this.DocumentsDownloadedList.Add(doc);

            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences(DOCUMENT_PREFERENCES_SAVED_KEY, FileCreationMode.Private);

            // convert the list to json
            var listOfCustomersAsJson = JsonConvert.SerializeObject(this.DocumentsDownloadedList);

            ISharedPreferencesEditor editor = pref.Edit();

            // set the value to Customers key
            editor.PutString(DOCUMENT_SAVED_KEY, listOfCustomersAsJson);

            // commit the changes
            editor.Commit();

        }

        #endregion

    }

    class PagerAdapter : FragmentPagerAdapter
    {
        List<string> tabTitles;

        public PagerAdapter(global::AndroidX.Fragment.App.FragmentManager fm, List<string> titles) : base(fm)
        {
            this.tabTitles = titles;
        }

        public override int Count => tabTitles.Count;

        public override global::AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            var item = tabTitles[position];
            switch (item)
            {
                case "Guardias":
                    return ShortDialFragment.newInstance();
                case "Eventos":
                    return EventsFragment.newInstance();
                case "Novedades":
                    return FeaturesFragment.newInstance();
                case "Documentos":
                    return DocumentsFragment.newInstance();
                default:
                    break;
            }

            return null;
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(tabTitles[position]);
        }


    }

}
