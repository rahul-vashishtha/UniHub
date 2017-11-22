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
using Android.Support.V7.App;
using Android.Graphics;
using Android.Animation;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Views.Animations;
using Android.Support.Design.Widget;
using BartoszLipinski;
using System.Threading;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ImageViewerActivity : ExtendedAppCompatActivity
    {
        ViewStub vs;
        View view;
        ObjectAnimator beatAnimation;
        Bitmap bitmap;
        ImageView imageView;
        TextView title;
        WebClient wc;
        Uri fileUri;
        FloatingActionButton fabMain, fabAction, fabShare;
        Animation fabOpen, fabClose, rotateForward, rotateBackward, loader;

        string filePath;
        bool isFileOffline = false, downloadComplete = false, isFabOpen = false;
        byte[] bytes;
        int titleShrinkSize, titleExpandSize;

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

            SetContentView(Resource.Layout.ImageViewer);

            Bundle extras = this.Intent.Extras;
            isFileOffline = extras.GetBoolean("IsFileOffline");

            if (isFileOffline)
            {
                filePath = extras.GetString("PATH");
            }
            else
            {
                fileUri = new Uri(extras.GetString("URL"));
            }

            titleExpandSize = Functions.ConvertToPixels(this, 66);
            titleShrinkSize = Functions.ConvertToPixels(this, 186);

            SetUpViews();
            SetupLoaderAnimation();

            StartProcess();
        }

        private void StartProcess()
        {
            if(isFileOffline)
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
                    string _imageFileName = "tempImage.jpg";
                    string _imagePath = System.IO.Path.Combine(_documentsPath, _imageFileName);

                    bytes = wc.DownloadData(fileUri);

                    if (bytes != null && bytes.Length > 0)
                    {
                        bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                    }

                    File.WriteAllBytes(_imagePath, bytes);
                    downloadComplete = true;

                    RunOnUiThread(() =>
                    {
                        while (this.CurrentState == ActivityState.Pause) ;

                        imageView.SetImageBitmap(bitmap);
                        ShowLoader = false;

                        AnimateUI();
                    });
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
                            Thread.Sleep(1000);
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

        private async void ViewOfflineFile()
        {
            await Task.Run(() =>
            {
                try { Looper.Prepare(); }
                catch { }

                Looper looper = Looper.MyLooper();

                bitmap = BitmapFactory.DecodeFile(new Java.IO.File(filePath).AbsolutePath);

                RunOnUiThread(() =>
                {
                    while (this.CurrentState == ActivityState.Pause) ;

                    imageView.SetImageBitmap(bitmap);
                    AnimateUI();
                });

                looper.Quit();
            });
        }

        private void DeleteFile()
        {
            try
            {
                File.Delete(filePath);
                Toast.MakeText(ApplicationContext, "File Successfully Deleted", ToastLength.Long).Show();
                Finish();
            }
            catch
            {
                Toast.MakeText(this, "Unable to delete file.", ToastLength.Short).Show();
            }
        }

        private void SaveFile()
        {
            if (downloadComplete)
            {
                try
                {
                    string fullFilePath = System.IO.Path.Combine(AppConstant.NoticePath, System.IO.Path.GetFileName(fileUri.ToString()));
                    File.WriteAllBytes(fullFilePath, bytes);

                    filePath = fullFilePath;
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
                title.StartAnimation(titleBar);
                imageView.StartAnimation(content);
                fabMain.StartAnimation(fab);

                imageView.Visibility = ViewStates.Visible;
                title.Visibility = ViewStates.Visible;
                fabMain.Visibility = ViewStates.Visible;
            });
        }

        private void SetUpViews()
        {
            imageView = FindViewById<ImageView>(Resource.Id.imgNotice);
            title = FindViewById<TextView>(Resource.Id.ImageViewerTitle);

            imageView.Visibility = ViewStates.Invisible;
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

            fabShare.Click += ((sender, args) =>
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
                fabAction.Click += new EventHandler((sender, args) =>
                {
                    SaveFile();
                });
            }

            Functions.ResizeText(title, this);
            Functions.SetElevation(6f, 12f, title);
            title.Typeface = Typeface.CreateFromAsset(Assets, "Fonts/OpenSans/OpenSans-Bold.ttf");
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

            if (isFileOffline)
            {
                Intent shareFile = new Intent(Intent.ActionSend);
                Java.IO.File fileWithinMyDir = new Java.IO.File(filePath);

                if (fileWithinMyDir.Exists())
                {
                    shareFile.SetType("image/*");

                    shareFile.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + filePath));
                    shareFile.PutExtra(Intent.ExtraText, "Shared via UniHub");
                    shareFile.SetFlags(ActivityFlags.ClearTop);
                    shareFile.PutExtra(Intent.ExtraSubject, "Hey! check out this notice released by our Institute");
                    StartActivity(Intent.CreateChooser(shareFile, "Share Notice"));
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
                shareFile.PutExtra(Intent.ExtraSubject, "Hey! Check out this notice released by our Institute.");
                StartActivity(Intent.CreateChooser(shareFile, "Share Notice"));
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