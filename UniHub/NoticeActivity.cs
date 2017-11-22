
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
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Net;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using SystemPath = System.IO.Path;
using Android.Util;
using Android.Views.Animations;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class NoticeActivity : AppCompatActivity
    {
        SwipeRefreshLayout swipe;
        List<Notices> notices = null;
        NoticeAdapter noticeAdapter;
        ListView list;

        bool ShowLoader
        {
            get
            {
                return swipe.Refreshing;
            }
            set
            {
                try
                {
                    RunOnUiThread(() =>
                    {
                        swipe.Post(() =>
                        {
                            swipe.Refreshing = value;   
                        });
                    });
                }
                catch { }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);

            SetContentView(Resource.Layout.Notices);

            list = FindViewById<ListView>(Resource.Id.listNotices);
            swipe = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefresh);

            list.LayoutAnimation = AnimationUtils.LoadLayoutAnimation(this, Resource.Animation.LayoutAnimatorFade);
            list.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(List_ItemClick);

            swipe.Refresh += ((sender, args) =>
            {
                notices = null;
                StartProcess();
            });

            StartProcess();
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            LaunchNotice(e.Position);
        }

        private void StartProcess()
        {
            if(!new NetworkProfile(this).IsSystemOnline)
            {
                if (IsFinishing)
                    return;

                new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog))
                .SetPositiveButton("Retry", (sender, args) =>
                {
                    StartProcess();
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
            else
            {
                ShowLoader = true;
                RetrieveNoticeData();
            }
        }

        private async void RetrieveNoticeData()
        {
            await Task.Run(() =>
            {
                try
                {
                    Looper.Prepare();
                }
                catch { }

                Looper l = Looper.MyLooper();

                XDocument doc = null;

                try
                {
                    if (notices == null)
                    {
                        WebClient wc = new WebClient();
                        wc.Headers.Add("User-Agent: UniHub");
                        wc.Proxy = null;
                        doc = XDocument.Parse(wc.DownloadString("http://abesit.in/category/notices/feed/"));
                    }
                    else
                    {
                        ShowNotices();
                        l.Quit();

                        return;
                    }
                }
                catch
                {
                    RunOnUiThread(() =>
                    {
                        ShowLoader = false;

                        if (IsFinishing)
                            return;

                        new AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.Dialog))
                        .SetPositiveButton("Okay", (sender, args) =>
                        {
                            l.Quit();

                            this.Finish();
                        })
                        .SetCancelable(false)
                        .SetMessage("Unable to connect to services. Please check your internet connection and try again.")
                        .SetTitle("Connection Error")
                        .Show();

                        return;
                    });
                }

                notices = new List<Notices>();

                if (doc != null)
                {
                    notices = ParseData.ReadNotices.GetNotices(ApplicationContext, doc);
                }
                else
                {
                    ShowLoader = false;

                    l.Quit();

                    StartProcess();

                    return;
                }

                ShowNotices();              // Populate The Notices List

                l.Quit();
            });
        }

        private void ShowNotices()
        {
            RunOnUiThread(() =>
            {
                noticeAdapter = new NoticeAdapter(this, notices);
                list.Adapter = noticeAdapter;
                list.StartLayoutAnimation();

                ShowLoader = false;
            });
        }

        private void LaunchNotice(int index)
        {
            Bundle bundle;
            Android.Content.Intent i;

            if (notices[index].HasImage)
            {
                if (AppConstant.IsNoticeOffline(SystemPath.GetFileName(notices[index].ImageLink.ToString())))
                {
                    bundle = new Bundle();
                    bundle.PutBoolean("IsFileOffline", true);
                    bundle.PutString("PATH", SystemPath.Combine(AppConstant.NoticePath, SystemPath.GetFileName(notices[index].ImageLink.ToString())));
                }
                else
                {
                    bundle = new Bundle();
                    bundle.PutBoolean("IsFileOffline", false);
                    bundle.PutString("URL", notices[index].ImageLink.ToString());
                }

                i = new Android.Content.Intent(this, typeof(ImageViewerActivity));
            }
            else
            {
                if (AppConstant.IsNoticeOffline(SystemPath.GetFileName(notices[index].FileLink.ToString())))
                {
                    bundle = new Bundle();
                    bundle.PutBoolean("IsFileOffline", true);
                    bundle.PutString("PATH", SystemPath.Combine(AppConstant.NoticePath, SystemPath.GetFileName(notices[index].FileLink.ToString())));
                }
                else
                {
                    bundle = new Bundle();
                    bundle.PutBoolean("IsFileOffline", false);
                    bundle.PutString("URL", notices[index].FileLink.ToString());
                    bundle.PutBoolean("IsFileNotice", true);
                }

                i = new Android.Content.Intent(this, typeof(PDFViewerActivity));
            }

            if (Looper.MyLooper() == null)
                Looper.Prepare();

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