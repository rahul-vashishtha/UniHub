using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Views;
using Android.Animation;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.V7.App;
using Android.Views.Animations;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        TextView selectorAsk, selectorNews, selectorNotices, selectorPlacements, selectorQP, selectorOffline, selectorAbout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            AppConstant.SetUpDirectories();

            SetupTextView();

            InitializeEvents();

            AnimateViews();
        }

        private void InitializeEvents()
        {
            selectorNews.Click += ((sender, args) =>
            {
                OpenNews();
            });

            selectorNotices.Click += ((sender, args) =>
            {
                OpenNotice();
            });

            selectorPlacements.Click += ((sender, args) =>
            {
                OpenPlacements();
            });

            selectorQP.Click += ((sender, args) =>
            {
                OpenQP();
            });

            selectorOffline.Click += ((sender, args) =>
            {
                OpenOffline();
            });

            selectorAbout.Click += ((sender, args) =>
            {
                OpenAbout();
            });
        }

        private void SetupTextView()
        {
            // Initialize Variables
            selectorAsk = FindViewById<TextView>(Resource.Id.selector_ask);
            selectorNews = FindViewById<TextView>(Resource.Id.selectorNews);
            selectorNotices = FindViewById<TextView>(Resource.Id.selectorNotices);
            selectorPlacements = FindViewById<TextView>(Resource.Id.selectorPlacements);
            selectorQP = FindViewById<TextView>(Resource.Id.selectorQP);
            selectorOffline = FindViewById<TextView>(Resource.Id.selectorOffline);
            selectorAbout = FindViewById<TextView>(Resource.Id.selectorAbout);

            // Resize text according to screen density
            Functions.ResizeText(selectorAsk, this);
            Functions.ResizeText(selectorNews, this);
            Functions.ResizeText(selectorNotices, this);
            Functions.ResizeText(selectorPlacements, this);
            Functions.ResizeText(selectorQP, this);
            Functions.ResizeText(selectorOffline, this);
            Functions.ResizeText(selectorAbout, this);

            // Set elevation for API 21+ devices
            selectorAsk.SetShadowLayer(12f, -2f, 2f, Color.DarkGray);
            Functions.SetElevation(6f, 12f, selectorNews, selectorNotices, selectorPlacements, selectorQP, selectorOffline, selectorAbout);

            // Put the typeface properties
            selectorAsk.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorNews.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorNotices.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorPlacements.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorQP.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorOffline.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            selectorAbout.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
        }

        private void OpenNews()
        {
            if (new NetworkProfile(ApplicationContext).IsSystemOnline)
            {
                Toast.MakeText(this, "Opening News", ToastLength.Short).Show();

                Android.Content.Intent i = new Android.Content.Intent(this, typeof(NewsActivity));
                StartActivity(i);
            }
            else
            {
                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("Retry", (sender, args) =>
                {
                    OpenNotice();
                })
                .SetNegativeButton("Close", (sender, args) =>
                {
                    this.FinishAffinity();
                })
                .SetCancelable(false)
                .SetMessage("Unable to connect to internet. Please check your internet connection and try again.")
                .SetTitle("Connection Error")
                .Show();
            }
        }

        private void OpenNotice()
        {
            if (new NetworkProfile(ApplicationContext).IsSystemOnline)
            {
                Toast.MakeText(this, "Opening Notices", ToastLength.Short).Show();

                Android.Content.Intent i = new Android.Content.Intent(this, typeof(NoticeActivity));
                StartActivity(i);
            }
            else
            {
                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("Retry", (sender, args) =>
                {
                    OpenNotice();
                })
                .SetNegativeButton("Close", (sender, args) =>
                {
                    this.FinishAffinity();
                })
                .SetCancelable(false)
                .SetMessage("Unable to connect to internet. Please check your internet connection and try again.")
                .SetTitle("Connection Error")
                .Show();
            }
        }

        private void OpenPlacements()
        {
            if (new NetworkProfile(ApplicationContext).IsSystemOnline)
            {
                Toast.MakeText(this, "Opening Placements", ToastLength.Short).Show();

                Android.Content.Intent i = new Android.Content.Intent(this, typeof(PlacementActivity));
                StartActivity(i);
            }
            else
            {
                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("Retry", (sender, args) =>
                {
                    OpenNotice();
                })
                .SetNegativeButton("Close", (sender, args) =>
                {
                    this.FinishAffinity();
                })
                .SetCancelable(false)
                .SetMessage("Unable to connect to internet. Please check your internet connection and try again.")
                .SetTitle("Connection Error")
                .Show();
            }
        }

        private void OpenQP()
        {
            Toast.MakeText(this, "Opening Question Papers", ToastLength.Short).Show();

            Android.Content.Intent i = new Android.Content.Intent(this, typeof(QPActivity));
            StartActivity(i);
        }

        private void OpenOffline()
        {
            Toast.MakeText(this, "Opening Offline Files", ToastLength.Long).Show();

            Android.Content.Intent i = new Android.Content.Intent(this, typeof(OfflineFilesActivity));
            StartActivity(i);
        }

        private void OpenAbout()
        {
            Toast.MakeText(this, "Opening About", ToastLength.Long).Show();

            Android.Content.Intent i = new Android.Content.Intent(this, typeof(AboutActivity));
            StartActivity(i);
        }

        private void AnimateViews()
        {
            Animation ask = AnimationUtils.LoadAnimation(this, Resource.Animation.FadeIn);
            Animation news = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation notice = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation placement = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation qp = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation offline = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            Animation about = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInRight);

            ask.Interpolator = new DecelerateInterpolator();
            ask.StartOffset = 0;

            news.Interpolator = new DecelerateInterpolator();
            news.StartOffset = 150;

            notice.Interpolator = new DecelerateInterpolator();
            notice.StartOffset = 300;

            placement.Interpolator = new DecelerateInterpolator();
            placement.StartOffset = 450;

            qp.Interpolator = new DecelerateInterpolator();
            qp.StartOffset = 600;

            offline.Interpolator = new DecelerateInterpolator();
            offline.StartOffset = 750;

            about.Interpolator = new DecelerateInterpolator();
            about.StartOffset = 900;

            selectorAsk.StartAnimation(ask);
            selectorNews.StartAnimation(news);
            selectorNotices.StartAnimation(notice);
            selectorPlacements.StartAnimation(placement);
            selectorQP.StartAnimation(qp);
            selectorOffline.StartAnimation(offline);
            selectorAbout.StartAnimation(about);
        }
    }
}