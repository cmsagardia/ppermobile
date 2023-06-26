using Android;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Aysa.PPEMobile.Droid.Fragments;
using Android.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using Android.Content;


namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Theme = "@style/AppThemeWithActionBar", Label = "@string/app_name", Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        static readonly string Tag = "ActionBarTabsSupport";

        Fragment[] _fragments;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.ActionBar);

            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            _fragments = new Fragment[]
                         {
                             new ShortDialFragment(),
                new EventsFragment(),
                new DocumentsFragment()
                         };

            AddTabToActionBar(Resource.String.shortdial_tab_label);
            AddTabToActionBar(Resource.String.events_tab_label);
            AddTabToActionBar(Resource.String.documents_tab_label);

            SetContentView(Resource.Layout.Main);
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.MenuMain, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {

            // Change text color in menu
            IMenuItem item = menu.FindItem(Resource.Id.close_session);
            SpannableString s = new SpannableString(item.TitleCondensedFormatted);
            s.SetSpan(new ForegroundColorSpan(Color.ParseColor("#672E8A")), 0, s.Length(), 0);

            item.SetTitle(s);

            bool result =  base.OnPrepareOptionsMenu(menu);

            return result;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.close_session:
                    
                    // Close session
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);  
                    AlertDialog alert = dialog.Create();  
                    alert.SetTitle("Aviso");  
                    alert.SetMessage("¿Seguro que desea cerrar la sesión?");  
                    alert.SetButton("Si", (c, ev) =>  
                    {  
                        Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
                        StartActivity(shortDialDetails); 
                    }); 

                    alert.SetButton2("Cancelar", (c, ev) =>  { }); 

                    alert.Show();  

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        void AddTabToActionBar(int labelResourceId)
        {
            ActionBar.Tab tab = ActionBar.NewTab()
                .SetText(labelResourceId);
            tab.TabSelected += TabOnTabSelected;
            ActionBar.AddTab(tab);
        }

        void TabOnTabSelected(object sender, ActionBar.TabEventArgs tabEventArgs)
        {
            ActionBar.Tab tab = (ActionBar.Tab)sender;

            Log.Debug(Tag, "The tab {0} has been selected.", tab.Text);
            Fragment frag = _fragments[tab.Position];
            tabEventArgs.FragmentTransaction.Replace(Resource.Id.frameLayout1, frag);
        }
    }
}
