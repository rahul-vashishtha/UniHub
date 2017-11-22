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
using Android.Graphics;
using Android.Views.Animations;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OfflineFilesActivity : ExtendedAppCompatActivity
    {
        TextView questionPapers, notices;
        ListView offlineFiles = null;
        List<OfflineFile> listOffline = null;

        enum OfflineCategory { QP, Notice };
        OfflineCategory CurrentShowing = OfflineCategory.Notice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);

            SetContentView(Resource.Layout.OfflineFiles);

            SetUpTextViews();

            ShowOfflineNotices();
        }

        private void SetUpTextViews()
        {
            questionPapers = FindViewById<TextView>(Resource.Id.BtnQP);
            notices = FindViewById<TextView>(Resource.Id.BtnNotices);

            offlineFiles = FindViewById<ListView>(Resource.Id.listOffline);
            offlineFiles.LayoutAnimation = AnimationUtils.LoadLayoutAnimation(this, Resource.Animation.LayoutAnimatorFade);
            offlineFiles.ItemClick += ((sender, args) =>
            {
                if (CurrentShowing == OfflineCategory.Notice)
                    LaunchOfflineNotice(args.Position);
                else
                    LaunchOfflineQP(args.Position);
            });

            Functions.ResizeText(questionPapers, this);
            Functions.ResizeText(notices, this);

            questionPapers.Typeface = Typeface.CreateFromAsset(this.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            notices.Typeface = Typeface.CreateFromAsset(this.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");

            questionPapers.Click += ((sender, args) =>
            {
                ShowOfflineQP();
            });

            notices.Click += ((sender, args) =>
            {
                ShowOfflineNotices();
            });

            Animation fromLeft = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            fromLeft.Interpolator = new DecelerateInterpolator();
            fromLeft.Duration = 750;
            fromLeft.StartOffset = 450;

            Animation fromRight = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInRight);
            fromRight.Interpolator = new DecelerateInterpolator();
            fromRight.Duration = 750;
            fromLeft.StartOffset = 450;

            Functions.SetElevation(6f, 6f, questionPapers, notices);

            questionPapers.StartAnimation(fromLeft);
            notices.StartAnimation(fromRight);
        }

        private void ShowOfflineQP()
        {
            listOffline = AppConstant.GetOfflineQuestionPapers();

            if (listOffline.Count == 0)
            {
                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("OK", (sender, args) =>
                {
                    
                })
                .SetCancelable(false)
                .SetMessage("No offline question papers found.")
                .SetTitle("Not Found")
                .Show();
            }
            else
            {
                offlineFiles.Adapter = new OfflineFilesAdapter(this, listOffline, "Question Papers");
                CurrentShowing = OfflineCategory.QP;
                offlineFiles.StartLayoutAnimation();
            }
        }

        private void ShowOfflineNotices()
        {
            listOffline = AppConstant.GetOfflineNotices();

            if (listOffline.Count == 0)
            {
                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("OK", (sender, args) => {  })
                    .SetCancelable(false)
                    .SetMessage("No offline notices found.")
                    .SetTitle("Not Found")
                    .Show();
            }
            else
            {
                offlineFiles.Adapter = new OfflineFilesAdapter(this, listOffline, "Notices");
                CurrentShowing = OfflineCategory.Notice;
                offlineFiles.StartLayoutAnimation();
            }
        }

        private void LaunchOfflineNotice(int index)
        {
            Bundle bundle;
            Android.Content.Intent i;

            if (listOffline[index].FileExtension.ToLower().Contains("jpg") || listOffline[index].FileExtension.ToLower().Contains("png") || listOffline[index].FileExtension.ToLower().Contains("jpeg"))
            {
                bundle = new Bundle();
                bundle.PutBoolean("IsFileOffline", true);
                bundle.PutString("PATH", listOffline[index].FilePath);

                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                i = new Android.Content.Intent(this, typeof(ImageViewerActivity));
                i.SetFlags(Android.Content.ActivityFlags.NewTask | Android.Content.ActivityFlags.ClearTop);
                i.PutExtras(bundle);
                this.StartActivity(i);
            }
            else if (listOffline[index].FileExtension.ToLower().Contains("pdf"))
            {
                bundle = new Bundle();
                bundle.PutBoolean("IsFileOffline", true);
                bundle.PutString("PATH", listOffline[index].FilePath);

                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                i = new Android.Content.Intent(this, typeof(PDFViewerActivity));
                i.SetFlags(Android.Content.ActivityFlags.NewTask | Android.Content.ActivityFlags.ClearTop);
                i.PutExtras(bundle);
                this.StartActivity(i);
            }
        }

        private void LaunchOfflineQP(int index)
        {
            Bundle bundle = new Bundle();
            bundle.PutBoolean("IsFileOffline", true);
            bundle.PutString("PATH", listOffline[index].FilePath);

            if (Looper.MyLooper() == null)
                Looper.Prepare();

            Android.Content.Intent i = new Android.Content.Intent(this, typeof(PDFViewerActivity));
            i.SetFlags(Android.Content.ActivityFlags.NewTask | Android.Content.ActivityFlags.ClearTop);
            i.PutExtras(bundle);
            this.StartActivity(i);
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