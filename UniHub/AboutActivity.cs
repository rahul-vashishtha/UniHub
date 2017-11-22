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
using Android.Views.Animations;
using Android.Graphics;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class AboutActivity : Activity
    {
        List<About> about = new List<About>();
        LinearLayout baseLayout;
        int delay = 150;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInRight, Resource.Animation.SlideOutRight);

            SetContentView(Resource.Layout.About);

            baseLayout = FindViewById<LinearLayout>(Resource.Id.InfoBase);

            about.Add(new About() { FieldName = "App Name :", FieldValue = "UniHub" });
            about.Add(new About() { FieldName = "College :", FieldValue = "ABESIT" });
            about.Add(new About() { FieldName = "Developer :", FieldValue = "Rahul Vashishtha" });
            about.Add(new About() { FieldName = "Branch & Year :", FieldValue = "3rd Year, IT" });

            TextView tv = FindViewById<TextView>(Resource.Id.AboutApp);
            Functions.ResizeText(tv, this);
            tv.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            tv.SetShadowLayer(12f, -2f, 2f, Color.DarkGray);

            LoadAbout();
        }

        private void LoadAbout()
        {
            try
            {
                LayoutInflater inflator = LayoutInflater.FromContext(this);

                foreach (About a in about)
                {
                    LinearLayout layout = (LinearLayout)inflator.Inflate(Resource.Layout.AboutField, null, false);
                    TextView fieldName = layout.FindViewById<TextView>(Resource.Id.FieldName);
                    TextView fieldValue = layout.FindViewById<TextView>(Resource.Id.FieldValue);

                    fieldName.Text = a.FieldName.ToUpper();
                    fieldValue.Text = a.FieldValue.ToUpper();

                    SetUpTextView(fieldName, fieldValue);

                    baseLayout.AddView(layout);

                    AnimateView(fieldName, fieldValue);
                }
            }
            catch { }
        }

        private void SetUpTextView(TextView tvLeft, TextView tvRight)
        {
            Functions.ResizeText(tvLeft, this);
            Functions.ResizeText(tvRight, this);

            tvLeft.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            tvRight.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");

            // Set elevation for API 21+ devices
            tvLeft.SetShadowLayer(12f, -2f, 2f, Color.DarkGray);
            Functions.SetElevation(6f, 12f, tvRight);
        }

        private void AnimateView(TextView tvLeft, TextView tvRight)
        {
            Animation fromRight = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation fromLeft = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInRight);

            fromLeft.Interpolator = new DecelerateInterpolator();
            fromLeft.StartOffset = delay;

            fromRight.Interpolator = new DecelerateInterpolator();
            fromRight.StartOffset = delay;

            delay += 150;

            tvLeft.StartAnimation(fromLeft);
            tvRight.StartAnimation(fromRight);
        }

        public override void FinishAffinity()
        {
            base.FinishAffinity();
            OverridePendingTransition(Resource.Animation.SlideInRight, Resource.Animation.SlideOutRight);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.SlideInRight, Resource.Animation.SlideOutRight);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.SlideInRight, Resource.Animation.SlideOutRight);
        }
    }
}