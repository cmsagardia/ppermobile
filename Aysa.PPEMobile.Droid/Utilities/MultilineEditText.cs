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
using Android.Views.InputMethods;
using Android.Util;

namespace Aysa.PPEMobile.Droid.Utilities
{
    public class MultilineEditText : EditText
    {
        public MultilineEditText(Context context) : base(context)
        {
        }

        public MultilineEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public MultilineEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public MultilineEditText(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected MultilineEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
        {
            var inputConn = base.OnCreateInputConnection(outAttrs);


            int outAttrsImeOptions = (int)outAttrs.ImeOptions;
            int imeActions = (int)outAttrs.ImeOptions;
            imeActions = imeActions & (int)ImeAction.ImeMaskAction;

            //if (((int)ImeAction.Done & imeActions) != 0)
            //{
            //    // Clear de existing action
            //    outAttrsImeOptions ^= imeActions;

            //    // Set de NEXT Action
            //    outAttrsImeOptions |= (int)ImeAction.Next;

            //}
            // Clear "No Enter Action" flag
            if ((outAttrsImeOptions & (int)ImeFlags.NoEnterAction) != 0)
            {
                outAttrsImeOptions &= ~(int)ImeFlags.NoEnterAction;
            }
            outAttrs.ImeOptions = (ImeFlags)outAttrsImeOptions;

            return inputConn;
        }



    }
}