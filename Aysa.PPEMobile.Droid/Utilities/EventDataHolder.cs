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
    public class EventDataHolder
    {
        private Event data;
        public Event getData() { return data; }
        public void setData(Event data) { this.data = data; }


        private static EventDataHolder holder = new EventDataHolder();

        public static EventDataHolder getInstance()
        {
            return holder;
        }
    }
}