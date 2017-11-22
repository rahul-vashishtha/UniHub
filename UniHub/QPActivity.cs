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
using SystemPath = System.IO.Path;
using Android.Views.Animations;
using Android.Graphics;

namespace UniHub
{
    [Activity(Label = "UniHub", Theme = "@style/UniHubTheme", Icon = "@drawable/logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class QPActivity : ExtendedAppCompatActivity
    {
        List<Semester> semesters = null;
        ListView list;
        QuestionPapersAdapter qpAdapter = null;
        TextView sem, year, questionPaper;

        QPCategories CurrentShowingQPCategory { get; set; }
        int semesterIndex = 0, yearIndex = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            OverridePendingTransition(Resource.Animation.SlideInLeft, Resource.Animation.SlideOutLeft);

            SetContentView(Resource.Layout.QuestionPapers);

            list = FindViewById<ListView>(Resource.Id.listQP);
            list.LayoutAnimation = AnimationUtils.LoadLayoutAnimation(this, Resource.Animation.LayoutAnimatorFade);
            list.ItemClick += ((sender, args) =>
            {
                LaunchQP(args.Position);
            });

            SetUpTextViews();

            semesters = ParseData.ReadQuestionPapers.GetQuestionPapers(this);
            CurrentShowingQPCategory = QPCategories.Semester;

            ShowSemesters();
        }

        private void SetUpTextViews()
        {
            sem = FindViewById<TextView>(Resource.Id.SemesterPath);
            year = FindViewById<TextView>(Resource.Id.YearPath);
            questionPaper = FindViewById<TextView>(Resource.Id.QPPath);

            Functions.ResizeText(sem, this);
            Functions.ResizeText(year, this);
            Functions.ResizeText(questionPaper, this);

            sem.Typeface = Typeface.CreateFromAsset(this.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            year.Typeface = Typeface.CreateFromAsset(this.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            questionPaper.Typeface = Typeface.CreateFromAsset(this.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");

            sem.Visibility = ViewStates.Visible;
            year.Visibility = ViewStates.Gone;
            questionPaper.Visibility = ViewStates.Gone;

            sem.Click += ((sender, args) =>
             {
                 ShowSemesters();
             });

            year.Click += ((sender, args) =>
            {
                ShowYears(semesterIndex);
            });

            questionPaper.Click += ((sender, args) =>
            {
                ShowFiles(yearIndex);
            });
        }

        private void LaunchQP(int index)
        {
            switch (CurrentShowingQPCategory)
            {
                case QPCategories.Semester:
                    ShowYears(index);
                    break;

                case QPCategories.Year:
                    ShowFiles(index);
                    break;

                case QPCategories.QuestionPaper:
                    LaunchFile(index);
                    break;
            }
        }

        private void ShowSemesters()
        {
            qpAdapter = new QuestionPapersAdapter(this, semesters);

            list.Adapter = qpAdapter;
            list.StartLayoutAnimation();

            CurrentShowingQPCategory = QPCategories.Semester;

            sem.Visibility = ViewStates.Visible;
            year.Visibility = ViewStates.Gone;
            questionPaper.Visibility = ViewStates.Gone;
        }

        private void ShowYears(int index)
        {
            qpAdapter = new QuestionPapersAdapter(this, semesters[index].Years, semesters[index].SemesterName);

            list.Adapter = qpAdapter;
            list.StartLayoutAnimation();

            semesterIndex = index;

            CurrentShowingQPCategory = QPCategories.Year;

            sem.Visibility = ViewStates.Visible;
            year.Visibility = ViewStates.Visible;
            questionPaper.Visibility = ViewStates.Gone;
        }

        private void ShowFiles(int index)
        {
            qpAdapter = new QuestionPapersAdapter(this, semesters[semesterIndex].Years[index].QuestionPapers, semesters[semesterIndex].SemesterName, semesters[semesterIndex].Years[index].YearName);

            list.Adapter = qpAdapter;
            list.StartLayoutAnimation();

            yearIndex = index;

            CurrentShowingQPCategory = QPCategories.QuestionPaper;

            sem.Visibility = ViewStates.Visible;
            year.Visibility = ViewStates.Visible;
            questionPaper.Visibility = ViewStates.Visible;
        }

        private void LaunchFile(int index)
        {
            Bundle bundle;

            if (AppConstant.IsQPOffline(SystemPath.GetFileName(semesters[semesterIndex].Years[yearIndex].QuestionPapers[index].FileLink.ToString())))
            {
                bundle = new Bundle();
                bundle.PutBoolean("IsFileOffline", true);
                bundle.PutString("PATH", SystemPath.Combine(AppConstant.QPFolderPath, SystemPath.GetFileName(semesters[semesterIndex].Years[yearIndex].QuestionPapers[index].FileLink.ToString())));
            }
            else
            {
                bundle = new Bundle();
                bundle.PutBoolean("IsFileOffline", false);

                bundle.PutString("URL", semesters[semesterIndex].Years[yearIndex].QuestionPapers[index].FileLink.ToString());
                bundle.PutBoolean("IsFileNotice", false);
            }

            if (Looper.MyLooper() == null)
                Looper.Prepare();

            Android.Content.Intent i = new Android.Content.Intent(this, typeof(PDFViewerActivity));
            i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
            i.PutExtras(bundle);
            StartActivity(i);
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