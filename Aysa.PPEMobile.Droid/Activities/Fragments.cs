
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Aysa.PPEMobile.Droid.Activities
{
    public class SpeakersFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.simple_fragment, null);
            view.FindViewById<TextView>(Resource.Id.textView1).SetText(Resource.String.events_tab_label);
            view.FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(Resource.Drawable.ic_action_speakers);
            return view;
        }
    }
}
