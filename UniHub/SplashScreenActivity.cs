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
using System.Threading;
using Android.Views.Animations;
using Android.Support.V7.App;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", MainLauncher = true, NoHistory = true, Icon = "@drawable/logo", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance ,ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreenActivity : AppCompatActivity
    {
        ImageView logo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.ZoomIn, Resource.Animation.ZoomOut);

            SetContentView(Resource.Layout.SplashScreen);

            logo = FindViewById<ImageView>(Resource.Id.SplashLogo);

            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.ZoomIn);
            anim.Duration = 1000;
            logo.Animation = anim;
            logo.StartAnimation(anim);

            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(2000);

                LauchHomeScreen();
            });
        }

        private void LauchHomeScreen()
        {
            RunOnUiThread(() =>
               this.StartActivity(typeof(MainActivity)));
            Finish();
        }
    }
}