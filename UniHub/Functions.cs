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
using Android.Support.V4.View;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;

namespace UniHub
{
    class Functions
    {
        /// <summary>
        /// Resizes the text size of text view according to screen density of the device.
        /// </summary>
        /// <param name="tv">Text View of which the text size is to be resized.</param>
        /// <param name="context">Context for Text View.</param>
        public static void ResizeText(TextView tv, Context context)
        {
            float sourceTextSize = tv.TextSize;
            tv.SetTextSize(Android.Util.ComplexUnitType.Sp, sourceTextSize / context.Resources.DisplayMetrics.Density);
        }

        /// <summary>
        /// Apply elevation on the views for API 21+ devices.
        /// </summary>
        /// <param name="elevation">Size of the elevation.</param>
        /// <param name="views">Views on which the elevation is to be applied.</param>
        public static void SetElevation(float elevation, params View[] views)
        {
            foreach (View view in views)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    ViewCompat.SetElevation(view, elevation);
                }
            }
        }

        /// <summary>
        /// Apply elevation and translationZ on the views for API 21+ devices.
        /// </summary>
        /// <param name="elevation">Size of the elevation.</param>
        /// <param name="translationZ">Size of the translationZ.</param>
        /// <param name="views">Views on which the elevation is to be applied.</param>
        public static void SetElevation(float elevation, float translationZ, params View[] views)
        {
            foreach (View view in views)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    ViewCompat.SetElevation(view, elevation);
                    ViewCompat.SetTranslationZ(view, translationZ);
                }
            }
        }

        public static int ConvertToPixels(Context c, int value)
        {
            float d = c.Resources.DisplayMetrics.Density;
            return (int)(value * d); // margin in pixels
        }
    }

    //  Image Bitmap Related Functions Profile
    #region Image Function
    public static class ImageFunctions
    {
        public static Bitmap GetBitmapFromResources(Resources resources, int resourceId)
        {
            BitmapFactory.Options opts = new BitmapFactory.Options()
            {
                InJustDecodeBounds = false,
                InDither = false,
                InSampleSize = 1,
                InScaled = false,
                InPreferredConfig = Bitmap.Config.Argb8888
            };

            return ResizeImage(BitmapFactory.DecodeResource(resources, resourceId, opts), 48, 48);
        }

        public static Bitmap ResizeImage(Bitmap bmp, int newHeight, int newWidth)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            float scaleWidth = ((float)newWidth) / width;
            float scaleHeight = ((float)newHeight) / height;

            // CREATE A MATRIX FOR THE MANIPULATION
            Matrix matrix = new Matrix();

            // RESIZE THE BIT MAP
            matrix.PostScale(scaleWidth, scaleHeight);

            // "RECREATE" THE NEW BITMAP
            Bitmap newBitmap = Bitmap.CreateBitmap(bmp, 0, 0, width, height, matrix, true);
            return newBitmap;
        }

        public static Bitmap ResizeImageUsingCanvas(Bitmap bitmap, int newWidth, int newHeight)
        {
            Bitmap scaledBitmap = Bitmap.CreateBitmap(newWidth, newHeight, Bitmap.Config.Argb8888);

            float ratioX = newWidth / (float)bitmap.Width;
            float ratioY = newHeight / (float)bitmap.Height;
            float middleX = newWidth / 2.0f;
            float middleY = newHeight / 2.0f;

            Matrix scaleMatrix = new Matrix();
            scaleMatrix.SetScale(ratioX, ratioY, middleX, middleY);

            Canvas canvas = new Canvas(scaledBitmap);
            canvas.Matrix.Set(scaleMatrix);
            canvas.DrawBitmap(bitmap, middleX - bitmap.Width / 2, middleY - bitmap.Height / 2, new Paint(PaintFlags.AntiAlias));

            return scaledBitmap;

        }
    }
    #endregion

    // Network Related Functions Profile
    #region Network Profile
    public class NetworkProfile
    {
        Context context;

        public NetworkProfile(Context c)
        {
            context = c;
        }

        public bool IsSystemOnline
        {
            get
            {
                Android.Net.ConnectivityManager connectivity = (Android.Net.ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                Android.Net.NetworkInfo connection = connectivity.ActiveNetworkInfo;
                return (connection != null) && connection.IsConnected;
            }
        }
    }
    #endregion
}