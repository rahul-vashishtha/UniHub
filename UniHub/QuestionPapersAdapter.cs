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
using Android.Graphics.Drawables;
using System.ComponentModel;
using Square.Picasso;
using Android.Content.Res;
using Android.Views.Animations;

namespace UniHub
{
    class QuestionPapersAdapter : BaseAdapter<Semester>
    {
        List<Semester> semesters;
        List<Year> years;
        List<QuestionPaper> questionPapers;
        Context context;
        NoticeViewHolder viewHolder = null;

        string semester, year;

        QPCategories ShowCategory { get; set; }

        public QuestionPapersAdapter(Context c, List<Semester> items)
        {
            context = c;
            semesters = items;

            ShowCategory = QPCategories.Semester;
        }

        public QuestionPapersAdapter(Context c, List<Year> items, string sem)
        {
            context = c;
            years = items;
            semester = sem;

            ShowCategory = QPCategories.Year;
        }

        public QuestionPapersAdapter(Context c, List<QuestionPaper> items, string sem, string year)
        {
            context = c;
            questionPapers = items;
            semester = sem;
            this.year = year;

            ShowCategory = QPCategories.QuestionPaper;
        }

        public override int Count
        {
            get
            {
                if (ShowCategory == QPCategories.Semester)
                    return semesters.Count;

                else if (ShowCategory == QPCategories.Year)
                    return years.Count;

                else
                    return questionPapers.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Semester this[int position]
        {
            get { return semesters[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                viewHolder = new  NoticeViewHolder();
                convertView = LayoutInflater.FromContext(context).Inflate(Resource.Layout.NoticeList, null);

                viewHolder.Title = convertView.FindViewById<TextView>(Resource.Id.lstTxtTitle);
                viewHolder.Description = convertView.FindViewById<TextView>(Resource.Id.lstTxtDescription);
                viewHolder.Icon = convertView.FindViewById<TextView>(Resource.Id.lstTxtExt);
                viewHolder.BaseCover = convertView.FindViewById<LinearLayout>(Resource.Id.listCover);
                viewHolder.BaseIcon = convertView.FindViewById<LinearLayout>(Resource.Id.lstIcon);

                Functions.SetElevation(6f, 12f, viewHolder.BaseIcon, viewHolder.BaseCover);

                viewHolder.Title.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
                viewHolder.Description.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Regular.ttf");
                viewHolder.Icon.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");

                Functions.ResizeText(viewHolder.Title, context);
                Functions.ResizeText(viewHolder.Description, context);
                Functions.ResizeText(viewHolder.Icon, context);

                viewHolder.BaseIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#FF4040"));

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = convertView.Tag as NoticeViewHolder;
            }

            if (ShowCategory == QPCategories.Semester)
            {
                SetSemester(viewHolder, position);
            }
            else if (ShowCategory == QPCategories.Year)
            {
                SetYear(viewHolder, position);
            }
            else if (ShowCategory == QPCategories.QuestionPaper)
            {
                SetFile(viewHolder, position);
            }

            viewHolder.Icon.Text = "PDF";

            Animation iconAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.SlideInRight);
            iconAnimation.Duration = 500;
            iconAnimation.Interpolator = new DecelerateInterpolator(1.2f);

            Animation contentAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.SlideInLeft);
            contentAnimation.Duration = 500;
            contentAnimation.Interpolator = new DecelerateInterpolator(1.2f);

            AnimationSet set = new AnimationSet(false);
            set.AddAnimation(iconAnimation);
            set.AddAnimation(contentAnimation);

            convertView.FindViewById<LinearLayout>(Resource.Id.lstIcon).Animation = iconAnimation;
            convertView.FindViewById<LinearLayout>(Resource.Id.listCover).Animation = contentAnimation;
            convertView.StartAnimation(set);

            return convertView;
        }

        //  Choose Category and SetData
        #region Choose Category And Set Data 

        private void SetSemester(NoticeViewHolder viewHolder, int position)
        {
            viewHolder.Title.Text = " Semester : " + semesters[position].SemesterName;
            Functions.ResizeText(viewHolder.Title, context);

            viewHolder.Description.Text = "Course : B. Tech.";
            Functions.ResizeText(viewHolder.Description, context);
        }

        private void SetYear(NoticeViewHolder viewHolder, int position)
        {
            viewHolder.Title.Text = " Year : " + years[position].YearName;
            //viewHolder.Title.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            Functions.ResizeText(viewHolder.Title, context);

            viewHolder.Description.Text = "Course : B. Tech. Semester : " + semester;
            //viewHolder.Description.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Regular.ttf");
            Functions.ResizeText(viewHolder.Description, context);
        }

        private void SetFile(NoticeViewHolder viewHolder, int position)
        {
            viewHolder.Title.Text = " " + questionPapers[position].FileName;
            //viewHolder.Title.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
            Functions.ResizeText(viewHolder.Title, context);

            viewHolder.Description.Text = "Course : B. Tech. Semester : " + semester + " Year : " + year;
            //viewHolder.Description.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Regular.ttf");
            Functions.ResizeText(viewHolder.Description, context);
        }
        #endregion
    }
}