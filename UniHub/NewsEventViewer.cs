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
using Android.Support.V7.Widget;
using Square.Picasso;
using System.Collections;
using Android.Util;
using Android.Graphics;
using Android.Text;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class NewsEventViewer : Activity
    {
        ViewStub vs;
        View view;
        Animation loader;
        LinearLayout baseLayout;
        TextView tv;

        public bool ShowLoader
        {
            set
            {
                if (value == true)
                {
                    RunOnUiThread(() =>
                    {
                        view.Visibility = ViewStates.Visible;
                    });
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        view.StartAnimation(loader);
                        view.Visibility = ViewStates.Gone;
                    });
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);

            SetContentView(Resource.Layout.NewsViewer);

            SetUpViews();

            SetLoaderAnimation();

            List<Uri> images = ConvertToUriList(Intent.GetStringArrayListExtra("IMAGES"));
            tv.Text = Html.FromHtml(Intent.GetStringExtra("CONTENT")).ToString();

            AddImages(images);
        }

        private void SetUpViews()
        {
            baseLayout = FindViewById<LinearLayout>(Resource.Id.NewsBaseLayout);
            tv = FindViewById<TextView>(Resource.Id.NewsContent);

            Functions.ResizeText(tv, this);
            tv.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            tv.SetShadowLayer(9f, -1f, 1f, Color.DarkGray);
        }

        private List<Uri> ConvertToUriList(IList<string> list)
        {
            List<Uri> urls = new List<Uri>();

            foreach (string s in list)
            {
                urls.Add(new Uri(s));
            }

            return urls;
        }

        private void SetLoaderAnimation()
        {
            loader = AnimationUtils.LoadAnimation(this, Resource.Animation.FadeOut);
            loader.Interpolator = new DecelerateInterpolator();
            loader.Duration = 500;
        }

        private void AddImages(List<Uri> urls)
        {
            try
            {
                LayoutInflater inflator = LayoutInflater.FromContext(this);

                foreach (Uri uri in urls)
                {
                    LinearLayout layout = (LinearLayout)inflator.Inflate(Resource.Layout.ImageCard, null, false);
                    ImageView image = layout.FindViewById<ImageView>(Resource.Id.NewsImage);

                    baseLayout.AddView(layout);

                    Picasso.With(this).Load(uri.ToString()).Into(image);
                }
            }
            catch { }
        }

        public override void FinishAffinity()
        {
            base.FinishAffinity();
            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);
        }
    }
}