
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using AndroidX.AppCompat.Widget;
using Newtonsoft.Json;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.App;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "ShortDialDetailsActivity")]
    public class ShortDialDetailsActivity : AppCompatActivity
    {
        FrameLayout progressOverlay;

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        PersonInChargeListAdapter mAdapter;

        private TextView noPersonsTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ShortDialDetail);

            // Set toolbar and title
            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.Title = GetString(Resource.String.event_filter);


            // Add back button
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SetUpView();

            // Get Section selected
            Section sectionSelected = JsonConvert.DeserializeObject<Section>(Intent.GetStringExtra("sectionSelected"));

            FindViewById<TextView>(Resource.Id.txtSectionTitle).Text = sectionSelected.Nombre;

            noPersonsTextView = FindViewById<TextView>(Resource.Id.noPersonInCharge);

            // Get people in guard from the server
            GetPeopleInGuard(sectionSelected);
        }

        #region Private Methods

        private void SetUpView()
        {
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            InitTableView();
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

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void GetPeopleInGuard(Section sectionSelected)
        {
            // Get persons in guard to the section selected
            ShowProgressDialog(true);

            Task.Run(async () =>
            {

                try
                {
                    // Fill responsables guard 
                    List<PersonGuard> listGuard = await AysaClient.Instance.GetResponsablesGuardBySector(sectionSelected.Id);

                    RunOnUiThread(() =>
                    {
                        // Create an adapter for the RecyclerView, and pass it the
                        mAdapter = new PersonInChargeListAdapter(listGuard);

                        // Register the item click handler (below) with the adapter:
                        mAdapter.ItemClick += OnItemClick;

                        // Plug the adapter into the RecyclerView:
                        mRecyclerView.SetAdapter(mAdapter);

                        if(listGuard.Count == 0)
                        {
                            noPersonsTextView.Visibility = ViewStates.Visible;
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
                        ShowProgressDialog(false);
                    });
                }
            });
        }

        private void InitTableView()
        {
            // Get our RecyclerView layout:
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            // Layout Manager Setup:

            // Use the built-in linear layout manager:
            mLayoutManager = new LinearLayoutManager(this);

            // Plug the layout manager into the RecyclerView:
            mRecyclerView.SetLayoutManager(mLayoutManager);
        }

        // Handler for the item click event:
        void OnItemClick(object sender, ContactType contact)   
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.CallPhone) == (int)Permission.Granted)
            {
                Intent callIntent = new Intent(Intent.ActionCall);
                callIntent.SetData(Android.Net.Uri.Parse("tel:" + contact.NumberValue));

                StartActivity(callIntent);
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.CallPhone }, 1);
            }
                
        }

        #endregion

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }
    }

    // Implement the ViewHolder pattern: each ViewHolder holds references
    // to the UI components (ImageView and TextView) within the CardView 
    // that is displayed in a row of the RecyclerView:
    public class PersonInChargeViewHolder : RecyclerView.ViewHolder
    {
        //public ImageView Image { get; private set; }
        public TextView txtNameTitle { get; private set; }
        public LinearLayout lvlRowContainer { get; private set; }

        // Get references to the views defined in the CardView layout.
        public PersonInChargeViewHolder(View itemView): base(itemView)
        {
            txtNameTitle = itemView.FindViewById<TextView>(Resource.Id.txtName);
            lvlRowContainer = itemView.FindViewById<LinearLayout>(Resource.Id.lvlRowContainer);
            // Detect user clicks on the item view and report which item
            // was clicked (by layout position) to the listener:
            //itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }

        public void addSection(ContactType contact, bool isLastRowInSection, Action<ContactType> listener)
        {
            LayoutInflater inflater = (LayoutInflater)ItemView.Context.GetSystemService(Context.LayoutInflaterService);
            ViewGroup view = (ViewGroup)inflater.Inflate(Resource.Layout.ContactTypeRow, lvlRowContainer, false);

            view.FindViewById<TextView>(Resource.Id.txt_phone_number).Text = contact.NumberValue;

            // Define style of cell according to phone typle
            switch (contact.PhoneType)
            {
                case PhoneType.Office:
                    view.FindViewById<TextView>(Resource.Id.txt_phone_type).Text = "Oficina";
                    view.FindViewById<ImageView>(Resource.Id.img_type).SetImageResource(Resource.Drawable.office_phone);
                    break;
                case PhoneType.CellPhone:
                    view.FindViewById<TextView>(Resource.Id.txt_phone_type).Text = "Celular";
                    view.FindViewById<ImageView>(Resource.Id.img_type).SetImageResource(Resource.Drawable.cellphone);
                    break;
                case PhoneType.RPV:
                    view.FindViewById<TextView>(Resource.Id.txt_phone_type).Text = "RPV";
                    view.FindViewById<ImageView>(Resource.Id.img_type).SetImageResource(Resource.Drawable.rpv_phone);
                    break;
                case PhoneType.Alternative:
                    view.FindViewById<TextView>(Resource.Id.txt_phone_type).Text = "Alternativo";
                    view.FindViewById<ImageView>(Resource.Id.img_type).SetImageResource(Resource.Drawable.alternative_phone);
                    break;
                default:
                    break;
            }

            // Remove separator line
            if(isLastRowInSection) {
                //separator_line
                view.FindViewById<View>(Resource.Id.separator_line).Visibility = ViewStates.Gone;
            }

            view.Click += (sender, e) => listener(contact);

            //itemView.Click += (sender, e) => listener(base.LayoutPosition);

            lvlRowContainer.AddView(view);
        }
    }


    // Adapter to connect the data set (photo album) to the RecyclerView: 
    public class PersonInChargeListAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<ContactType> ItemClick;

        // Underlying data set (a photo album):
        public List<PersonGuard> mPersonInChargeList;

        public PersonInChargeListAdapter(List<PersonGuard> personsInChargeList)
        {
            mPersonInChargeList = personsInChargeList;
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PersonInChargeRow, parent, false);

            // Create a ViewHolder to find and hold these view references, and 
            // register OnClick with the view holder:
            PersonInChargeViewHolder vh = new PersonInChargeViewHolder(itemView);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            PersonInChargeViewHolder vh = holder as PersonInChargeViewHolder;
            PersonGuard personGuard = mPersonInChargeList[position];
            vh.txtNameTitle.Text = personGuard.NombreApellido;
            for (int i = 0; i < personGuard.ContactTypes.Count; i++)
            {
                ContactType contactType = personGuard.ContactTypes[i];

                // This flag is using to remove the separator line in the last row
                bool isLastRowInSection = i == personGuard.ContactTypes.Count - 1 ? true : false;

                vh.addSection(contactType, isLastRowInSection, OnClick);
            }

        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return mPersonInChargeList.Count; }
        }

        // Raise an event when the item-click takes place:
        void OnClick(ContactType contact)
        {
            if (ItemClick != null)
                ItemClick(this, contact);
        }
    }
}
