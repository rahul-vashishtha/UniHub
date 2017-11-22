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
using System.Net;
using System.Xml.Linq;
using Android.Util;
using Android.Support.V4.Widget;
using Android.Views.Animations;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using SystemPath = System.IO.Path;
using System.Threading;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PlacementActivity : ExtendedAppCompatActivity
    {
        SwipeRefreshLayout swipe;
        List<Placements> placementList = null;
        PlacementsAdapter placementAdapter;
        ListView list;

        private bool ShowLoader
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

            SetContentView(Resource.Layout.NewsEvents);

            list = FindViewById<ListView>(Resource.Id.listNotices);
            swipe = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefresh);

            list.LayoutAnimation = AnimationUtils.LoadLayoutAnimation(this, Resource.Animation.LayoutAnimatorFade);
            list.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(List_ItemClick);

            swipe.Refresh += ((sender, args) =>
            {
                placementList = null;
                StartProcess();
            });

            StartProcess();
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            LaunchPlacements(e.Position);
        }

        private void StartProcess()
        {
            if (!new NetworkProfile(this).IsSystemOnline)
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
                    this.Finish();
                })
                .SetCancelable(false)
                .SetMessage("Unable to connect to internet. Please check your internet connection and try again.")
                .SetTitle("Connection Error")
                .Show();
            }
            else
            {
                ShowLoader = true;
                RetrievePlacements();
            }
        }

        private async void RetrievePlacements()
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
                    if (placementList == null)
                    {
                        WebClient wc = new WebClient();
                        wc.Headers.Add("User-Agent: UniHub");
                        wc.Proxy = null;
                        doc = XDocument.Parse(wc.DownloadString("http://abesit.in/category/placement/feed/"));
                    }
                    else
                    {
                        ShowPlacements();
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

                        try
                        {
                            Thread.Sleep(1000);

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
                        }
                        catch
                        {
                            Finish();
                        }

                        return;
                    });
                }

                placementList = new List<Placements>();

                if (doc != null)
                {
                    placementList = ParseData.ReadPlacements.GetPlacements(ApplicationContext, doc);
                }
                else
                {
                    ShowLoader = false;

                    l.Quit();

                    StartProcess();

                    return;
                }

                ShowPlacements();              // Populate News & Events List

                l.Quit();
            });
        }

        private void ShowPlacements()
        {
            RunOnUiThread(() =>
            {
                placementAdapter = new PlacementsAdapter(this, placementList);
                list.Adapter = placementAdapter;
                list.StartLayoutAnimation();

                ShowLoader = false;
            });
        }

        private void LaunchPlacements(int position)
        {
            Bundle bundle = new Bundle();
            Intent i = new Android.Content.Intent(this, typeof(NewsEventViewer));
            IList<string> images = ConvertToStringList(placementList[position].Images);

            bundle.PutString("CONTENT", placementList[position].Content);
            bundle.PutStringArrayList("IMAGES", images);

            i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
            i.PutExtras(bundle);

            StartActivity(i);
        }

        private IList<string> ConvertToStringList(List<Uri> list)
        {
            IList<string> strings = new List<string>();

            foreach (Uri u in list)
            {
                strings.Add(u.ToString());
            }

            return strings;
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