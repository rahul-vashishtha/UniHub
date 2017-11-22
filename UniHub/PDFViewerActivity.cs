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
using Android.Animation;
using Android.Support.Design.Widget;
using Android.Graphics;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Webkit;
using Android.Util;
using BartoszLipinski;
using System.Threading;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PDFViewerActivity : ExtendedAppCompatActivity
    {
        ViewStub vs;
        View view;
        ObjectAnimator beatAnimation;
        FloatingActionButton fabMain, fabAction, fabShare;
        Animation fabOpen, fabClose, rotateForward, rotateBackward, loader;
        TextView title;
        Uri fileUri;
        WebClient wc;
        WebView pdfViewer;

        string filePath;
        bool isFileOffline = false, isFileNotice = true, downloadComplete = false, isFabOpen = false;
        byte[] bytes;
        int titleShrinkSize , titleExpandSize;

        public bool ShowLoader
        {
            set
            {
                if (value == true)
                {
                    RunOnUiThread(() =>
                    {
                        fabMain.Visibility = ViewStates.Gone;
                        view.Visibility = ViewStates.Visible;
                    });
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        loader = AnimationUtils.LoadAnimation(this, Resource.Animation.FadeOut);
                        loader.Interpolator = new DecelerateInterpolator();
                        loader.Duration = 500;

                        view.StartAnimation(loader);
                        view.Visibility = ViewStates.Gone;

                        fabMain.Visibility = ViewStates.Visible;
                    });
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);

            SetContentView(Resource.Layout.PDFViewer);

            Bundle extras = this.Intent.Extras;
            isFileOffline = extras.GetBoolean("IsFileOffline");

            if (isFileOffline)
            {
                filePath = extras.GetString("PATH");
            }
            else
            {
                fileUri = new Uri(extras.GetString("URL"));
                isFileNotice = extras.GetBoolean("IsFileNotice");
            }

            titleExpandSize = Functions.ConvertToPixels(this, 66);
            titleShrinkSize = Functions.ConvertToPixels(this, 186);

            SetUpViews();
            SetupLoaderAnimation();

            StartProcess();
        }

        private void StartProcess()
        {
            if (isFileOffline)
            {
                ViewOfflineFile();
            }
            else
            {
                if (new NetworkProfile(this).IsSystemOnline)
                {
                    StartDownload();
                }
                else
                {
                    if (IsFinishing)
                        return;

                    new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("Retry", (sender, args) =>
                    {
                        StartProcess();
                    }).SetNegativeButton("Cancel", (sender, args) =>
                    {
                        this.Finish();
                    }).SetCancelable(false).SetMessage("Unable to connect to internet. Please check your internet connection and try again.").SetTitle("Connection Error").Show();
                }
            }
        }

        private async void StartDownload()
        {
            await Task.Run(() =>
            {
                try { Looper.Prepare(); }
                catch { }

                Looper looper = Looper.MyLooper();

                try
                {
                    ShowLoader = true;

                    wc = new WebClient() { Proxy = null };
                    wc.Headers.Add("User-Agent: Notice Hunt");

                    string _documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                    string _pdfFileName = "tempPDF.pdf";
                    string _pdfPath = System.IO.Path.Combine(_documentsPath, _pdfFileName);

                    bytes = wc.DownloadData(fileUri);

                    if (File.Exists(_pdfPath))
                    {
                        try
                        {
                            File.Delete(_pdfPath);
                        }
                        catch { }
                    }

                    File.WriteAllBytes(_pdfPath, bytes);
                    downloadComplete = true;

                    ShowPDF(_pdfPath);
                }
                catch
                {
                    RunOnUiThread(() =>
                    {
                        if (IsFinishing)
                            return;

                        ShowLoader = false;

                        try
                        {
                            new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog)).SetPositiveButton("Retry", (sender, args) =>
                            {
                                looper.Quit();

                                StartProcess();
                            }).SetNegativeButton("Cancel", (sender, args) =>
                            {
                                this.Finish();
                            }).SetCancelable(false).SetMessage("Unable to connect to internet. Please check your internet connection and try again.").SetTitle("Connection Error").Show();
                        }
                        catch
                        {
                            Toast.MakeText(this, "Connection Error. Try Again.", ToastLength.Short);
                            Finish();
                        }
                    });
                }

                looper.Quit();
            });
        }

        private void ShowPDF(string path)
        {
            RunOnUiThread(() =>
            {
                while (this.CurrentState == ActivityState.Pause) ;

                ShowLoader = false;
                
                pdfViewer.LoadUrl("file:///android_asset/pdfviewer/index.html?file=" + path);

                AnimateUI();
            });
        }

        private async void ViewOfflineFile()
        {
            await Task.Run(() =>
            {
                try { Looper.Prepare(); }
                catch { }

                Looper looper = Looper.MyLooper();

                ShowLoader = true;

                Thread.Sleep(1000);

                ShowLoader = false;

                ShowPDF(filePath);

                looper.Quit();
            });
        }

        private void SetupLoaderAnimation()
        {
            beatAnimation = ObjectAnimator.OfPropertyValuesHolder(view.FindViewById<ImageView>(Resource.Id.imgLoaderLogo), PropertyValuesHolder.OfFloat("scaleX", 1.2f), PropertyValuesHolder.OfFloat("scaleY", 1.2f));
            beatAnimation.SetDuration(300);
            beatAnimation.RepeatCount = ValueAnimator.Infinite;
            beatAnimation.RepeatMode = ValueAnimatorRepeatMode.Reverse;
            beatAnimation.Start();
        }

        private void AnimateUI()
        {
            Animation titleBar = AnimationUtils.LoadAnimation(this, Resource.Animation.SlideInLeft);
            titleBar.Interpolator = new DecelerateInterpolator();
            titleBar.Duration = 1000;
            titleBar.StartOffset = 450;

            Animation content = AnimationUtils.LoadAnimation(this, Resource.Animation.UpFromBottom);
            content.Interpolator = new DecelerateInterpolator();
            content.Duration = 1000;
            content.StartOffset = 450;

            Animation fab = AnimationUtils.LoadAnimation(this, Resource.Animation.ZoomIn);
            fab.Interpolator = new DecelerateInterpolator();
            fab.Duration = 1000;
            fab.StartOffset = 450;

            RunOnUiThread(() =>
            {
                fabMain.StartAnimation(fab);
                title.StartAnimation(titleBar);
                pdfViewer.StartAnimation(content);

                fabMain.Visibility = ViewStates.Visible;
                pdfViewer.Visibility = ViewStates.Visible;
                title.Visibility = ViewStates.Visible;
            });
        }

        private void SetUpViews()
        {
            SetUpWebView();

            title = FindViewById<TextView>(Resource.Id.PDFViewerTitle);

            pdfViewer.Visibility = ViewStates.Invisible;
            title.Visibility = ViewStates.Invisible;

            vs = FindViewById<ViewStub>(Resource.Id.progressBarStub);
            vs.LayoutResource = Resource.Layout.PrograssBarLayout;
            view = vs.Inflate();
            view.BringToFront();

            fabMain = FindViewById<FloatingActionButton>(Resource.Id.fabMain);
            fabAction = FindViewById<FloatingActionButton>(Resource.Id.fabAction);
            fabShare = FindViewById<FloatingActionButton>(Resource.Id.fabShare);

            fabMain.Visibility = ViewStates.Invisible;

            fabOpen = AnimationUtils.LoadAnimation(this, Resource.Animation.FabOpen);
            fabClose = AnimationUtils.LoadAnimation(this, Resource.Animation.FabClose);
            rotateForward = AnimationUtils.LoadAnimation(this, Resource.Animation.RotateForward);
            rotateBackward = AnimationUtils.LoadAnimation(this, Resource.Animation.RotateBackward);

            fabMain.Click += ((sender, args) =>
            {
                AnimateFAB();
            });

            fabShare.Click += new EventHandler((sender, args) =>
            {
                Share();
            });

            if (isFileOffline)
            {
                fabAction.SetImageResource(Resource.Drawable.ic_delete_black_48dp);
                fabAction.Click += new EventHandler((sender, args) =>
                {
                    DeleteFile();
                });
            }
            else
            {
                fabAction.SetImageResource(Resource.Drawable.ic_file_download_black_48dp);
                fabAction.Click += new EventHandler((IntentSender, args) =>
                {
                    SaveFile();
                });
            }

            Functions.ResizeText(title, this);
            Functions.SetElevation(6f, 12f, title);
            title.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Bold.ttf");
        }

        private void SetUpWebView()
        {
            pdfViewer = FindViewById<WebView>(Resource.Id.pdfViewer);
            pdfViewer.SetWebChromeClient(new WebChromeClient());
            pdfViewer.SetInitialScale(1);
            pdfViewer.SetLayerType(LayerType.Software, null);

            pdfViewer.Settings.CacheMode = CacheModes.NoCache;
            pdfViewer.Settings.SetRenderPriority(WebSettings.RenderPriority.Normal);
            pdfViewer.Settings.JavaScriptEnabled = true;
            pdfViewer.Settings.AllowFileAccessFromFileURLs = true;
            pdfViewer.Settings.AllowUniversalAccessFromFileURLs = true;
            pdfViewer.Settings.BuiltInZoomControls = true;
            pdfViewer.Settings.UseWideViewPort = true;
            pdfViewer.Settings.LoadWithOverviewMode = true;

            pdfViewer.ClearCache(true);
        }

        private void AnimateFAB()
        {
            if (isFabOpen)
            {
                fabMain.StartAnimation(rotateBackward);

                ViewPropertyObjectAnimator.Animate(fabAction).LeftMargin(Functions.ConvertToPixels(this, 6)).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();
                ViewPropertyObjectAnimator.Animate(fabShare).LeftMargin(Functions.ConvertToPixels(this, 6)).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();
                ViewPropertyObjectAnimator.Animate(title).LeftMargin(titleExpandSize).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();

                fabClose.StartOffset = 300;

                fabAction.StartAnimation(fabClose);
                fabShare.StartAnimation(fabClose);

                fabAction.Clickable = false;
                fabShare.Clickable = false;

                isFabOpen = false;
            }
            else
            {
                fabAction.StartAnimation(fabOpen);
                fabShare.StartAnimation(fabOpen);

                ViewPropertyObjectAnimator.Animate(title).LeftMargin(titleShrinkSize).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();
                ViewPropertyObjectAnimator.Animate(fabAction).LeftMargin(Functions.ConvertToPixels(this, 66)).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();
                ViewPropertyObjectAnimator.Animate(fabShare).LeftMargin(Functions.ConvertToPixels(this, 126)).SetDuration(300).SetInterpolator(new DecelerateInterpolator()).Start();

                fabMain.StartAnimation(rotateForward);

                fabAction.Clickable = true;
                fabShare.Clickable = true;

                isFabOpen = true;
            }
        }

        private void DeleteFile()
        {
            try
            {
                File.Delete(filePath);
                Toast.MakeText(ApplicationContext, "File Successfully Deleted", ToastLength.Long).Show();
                Finish();
                //Snackbar.Make((View)FindViewById(Resource.Id.PDFViewerLayout), "Successfully Deleted.", Snackbar.LengthShort).Show();
            }
            catch
            {
                Snackbar.Make((View)FindViewById(Resource.Id.PDFViewerLayout), "Unable to delete file.", Snackbar.LengthLong).Show();
            }
        }

        private void SaveFile()
        {
            if (downloadComplete)
            {
                try
                {
                    if (isFileNotice)
                    {
                        string fullFilePath = System.IO.Path.Combine(AppConstant.NoticePath, System.IO.Path.GetFileName(fileUri.ToString()));
                        File.WriteAllBytes(fullFilePath, bytes);

                        filePath = fullFilePath;
                    }
                    else
                    {
                        string fullFilePath = System.IO.Path.Combine(AppConstant.QPFolderPath, System.IO.Path.GetFileName(fileUri.ToString()));
                        File.WriteAllBytes(fullFilePath, bytes);

                        filePath = fullFilePath;
                    }

                    isFileOffline = true;

                    UpdateFAB();

                    Toast.MakeText(this, "File Successfully Saved.", ToastLength.Short).Show();
                }
                catch
                {
                    Toast.MakeText(this, "Unable To Save.", ToastLength.Short).Show();
                }
            }
        }

        private void UpdateFAB()
        {
            fabAction.SetImageResource(Resource.Drawable.ic_delete_black_48dp);
            fabAction.Click += new EventHandler((sender, args) =>
            {
                DeleteFile();
            });
        }

        private void Share()
        {
            AnimateFAB();

            if(isFileOffline)
            {
                Intent shareFile = new Intent(Intent.ActionSend);
                Java.IO.File fileWithinMyDir = new Java.IO.File(filePath);

                if (fileWithinMyDir.Exists())
                {
                    shareFile.SetType("application/pdf");

                    shareFile.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + filePath));
                    shareFile.PutExtra(Intent.ExtraText, "Shared via UniHub");
                    shareFile.SetFlags(ActivityFlags.ClearTop);

                    if (isFileNotice)
                    {
                        shareFile.PutExtra(Intent.ExtraSubject, "Hey! check out this notice released by our Institute");
                        StartActivity(Intent.CreateChooser(shareFile, "Share Notice"));
                    }
                    else
                    {
                        shareFile.PutExtra(Intent.ExtraSubject, "Hey! Let's solve this Question Paper.");
                        StartActivity(Intent.CreateChooser(shareFile, "Share Question Paper"));
                    }
                }
                else
                {
                    Toast.MakeText(this, "File doesn't exists.", ToastLength.Short);
                }
            }
            else
            {
                Intent shareFile = new Intent(Intent.ActionSend);
                shareFile.SetType("text/plain");
                shareFile.AddFlags(ActivityFlags.ClearWhenTaskReset);
                shareFile.PutExtra(Intent.ExtraText, fileUri.ToString() + System.Environment.NewLine + "Shared via Unihub");

                if (isFileNotice)
                {
                    shareFile.PutExtra(Intent.ExtraSubject, "Hey! Check out this notice released by our Institute.");
                    StartActivity(Intent.CreateChooser(shareFile, "Share Notice"));
                }
                else
                {
                    shareFile.PutExtra(Intent.ExtraSubject, "Hey! Let's solve this Question Paper.");
                    StartActivity(Intent.CreateChooser(shareFile, "Share Question Paper"));
                }
            }
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