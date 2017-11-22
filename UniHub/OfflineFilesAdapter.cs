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
using Android.Content.Res;
using Android.Views.Animations;

namespace UniHub
{
    class OfflineFilesAdapter : BaseAdapter<OfflineFile>
    {
        List<OfflineFile> offlineFiles;
        Context context;
        NoticeViewHolder viewHolder;

        string category;

        public OfflineFilesAdapter(Context c, List<OfflineFile> list, string category)
        {
            context = c;
            offlineFiles = list;
            this.category = category;
        }

        public override OfflineFile this[int position]
        {
            get
            {
                return offlineFiles[position];
            }
        }

        public override int Count
        {
            get
            {
                return offlineFiles.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                viewHolder = new NoticeViewHolder();
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

                if(offlineFiles[position].FileExtension.ToUpper().Contains("PDF"))
                    viewHolder.BaseIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#FF4040"));
                else
                    viewHolder.BaseIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#FF840E"));

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (NoticeViewHolder)convertView.Tag;
            }

            viewHolder.Title.Text = " " + System.IO.Path.GetFileNameWithoutExtension(offlineFiles[position].FilePath);
            viewHolder.Description.Text = "Category : " + category;
            viewHolder.Icon.Text = offlineFiles[position].FileExtension.Replace(".", "").ToUpper();

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
    }
}