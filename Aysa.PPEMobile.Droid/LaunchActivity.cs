
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
using Aysa.PPEMobile.Droid.Activities;

namespace Aysa.PPEMobile.Droid
{
    [Activity(Label = "PPEMobile", MainLauncher = true)]
    public class LaunchActivity : Activity
    {
        private readonly long SPLASH_DISPLAY_LENGTH = 2200;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Launch);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Handler h = new Handler();
            Action myAction = () =>
            {
                var mainActivity = new Intent(this, typeof(LoginActivity));
                StartActivity(mainActivity);

                Finish();
            };

            h.PostDelayed(myAction, SPLASH_DISPLAY_LENGTH);
        }
    }
}
