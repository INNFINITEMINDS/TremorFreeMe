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

namespace TremorFreeMe
{
    [Activity(Label = "WearBot")]
    public class WearBot : Activity
    {
        TextView txt;
       users  main_user;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WearBot);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            txt = FindViewById<TextView>(Resource.Id.textView1);
            // Create your application here
            main_user = MainActivity.main_User;
            txt.Text ="qwerty"+ main_user.Fname;

        }
    }
}