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
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.Droid.Utilities
{
    public class FeatureDataHolder
    {
        private Feature data;
        public Feature getData() { return data; }
        public void setData(Feature data) { this.data = data; }


        private static FeatureDataHolder holder = new FeatureDataHolder();

        public static FeatureDataHolder getInstance()
        {
            return holder;
        }
    }
}