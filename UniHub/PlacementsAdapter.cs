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
using Android.Support.V7.Widget;
using Square.Picasso;
using Android.Views.Animations;
using Android.Graphics;

namespace UniHub
{
    class PlacementsAdapter : BaseAdapter<Placements>
    {
        List<Placements> placements = null;
        Context context;
        PlacementsViewHolder viewHolder;
        int lastPosition = -1;

        public PlacementsAdapter(Context c, List<Placements> placementsCollection)
        {
            context = c;
            placements = placementsCollection;
        }

        public override int Count
        {
            get
            {
                return placements.Count;
            }
        }

        public override Placements this[int position]
        {
            get
            {
                return placements[position];
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string pubDate = placements[position].PublishDate.Split(new string[] { " +" }, StringSplitOptions.None)[0];

            if (convertView == null)
            {
                viewHolder = new PlacementsViewHolder();
                convertView = LayoutInflater.FromContext(context).Inflate(Resource.Layout.NewsList, null);

                viewHolder.Title = convertView.FindViewById<TextView>(Resource.Id.NewsTitle);
                viewHolder.PublishDate = convertView.FindViewById<TextView>(Resource.Id.NewsPublishDate);
                viewHolder.Image = convertView.FindViewById<ImageView>(Resource.Id.NewsIcon);

                viewHolder.Title.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Semibold.ttf");
                viewHolder.PublishDate.Typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/OpenSans/OpenSans-Regular.ttf");

                Functions.ResizeText(viewHolder.Title, context);
                Functions.ResizeText(viewHolder.PublishDate, context);

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (PlacementsViewHolder)convertView.Tag;
            }

            viewHolder.Title.Text = placements[position].Title;
            viewHolder.PublishDate.Text = pubDate;

            Picasso.With(context).Load(placements[position].Icon.ToString()).Into(viewHolder.Image);

            Functions.SetElevation(6f, 6f, convertView);

            Animation listAnimation = AnimationUtils.LoadAnimation(context, (position > lastPosition) ? Resource.Animation.UpFromBottom : Resource.Animation.DownFromTop);
            listAnimation.Interpolator = new DecelerateInterpolator();
            convertView.StartAnimation(listAnimation);

            lastPosition = position;

            return convertView;
        }
    }
}