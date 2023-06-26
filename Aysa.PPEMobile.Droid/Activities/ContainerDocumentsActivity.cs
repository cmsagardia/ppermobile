
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Views;
using Android.Widget;
using Aysa.PPEMobile.Droid.Fragments;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "ContainerDocumentsActivity")]
    public class ContainerDocumentsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ContainerDocument);

            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);

            toolbar.Title = "Documentos Offline";
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            AndroidX.Fragment.App.FragmentTransaction fragmentTx = SupportFragmentManager.BeginTransaction();
            DocumentsFragment detailsFrag = new DocumentsFragment();
            fragmentTx.Add(Resource.Id.titles_fragment, detailsFrag);
            fragmentTx.Commit();
        }

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }
    }
}
