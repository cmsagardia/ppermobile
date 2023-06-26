using System;
using Android.Content;
using Android.Graphics;

namespace Aysa.PPEMobile.Droid.Utilities
{
    public class FontManager
    {
        public static String ROOT = "./";
        public static String FONTAWESOME = "fontawesome-webfont.ttf";
     
        private FontManager()
        {
        }

        public static Typeface getTypeface(Context context, String font) {
            return Typeface.CreateFromAsset(context.Assets, font);
        } 
    }
}
