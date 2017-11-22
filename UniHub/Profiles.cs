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
using System.IO;
using Android.Graphics;
using Android.Content.Res;
using Android.Support.V7.App;

namespace UniHub
{
    // Structure for Notice Profile
    #region Notice Profile
    public struct Notices
    {
        public string Title { get; set; }
        public Uri PageLink { get; set; }
        public string PublishDate { get; set; }
        public Uri ImageLink { get; set; }
        public Uri FileLink { get; set; }
        public bool HasDocument { get; set; }
        public bool HasImage { get; set; }

        public string Extension
        {
            get
            {
                if (HasImage && !HasDocument)
                {
                    return "IMG";
                }
                else
                {
                    return GetExtension();
                }
            }
        }

        private string GetExtension()
        {
            string ext = System.IO.Path.GetExtension(FileLink.ToString()).ToLower();

            if (ext.Contains("pdf"))
            {
                return "PDF";
            }
            else if (ext.Contains("doc") || ext.Contains("docx"))
            {
                return "DOC";
            }
            else if (ext.Contains("ppt") || ext.Contains("pptx"))
            {
                return "PPT";
            }
            else
            {
                return ext.ToUpper();
            }
        }
    }
    #endregion

    // Structure for Placement Profile
    #region Placement Profile
    public struct Placements
    {
        public string Title { get; set; }
        public string PublishDate { get; set; }
        public string Content { get; set; }
        public List<Uri> Images { get; set; }
        public Uri Icon { get; set; }
    }
    #endregion

    // Structure for News & Events Profile
    #region News & Events Profile
    public struct NewsEvents
    {
        public string Title { get; set; }
        public string PublishDate { get; set; }
        public string Content { get; set; }
        public List<Uri> Images { get; set; }
        public Uri Icon { get; set; }
    }
    #endregion

    // Structures for Question Papers
    #region Semester Structure
    struct Semester
    {
        public string SemesterName { get; set; }
        public List<Year> Years { get; set; }
    }

    struct Year
    {
        public string YearName { get; set; }
        public List<QuestionPaper> QuestionPapers { get; set; }
    }

    struct QuestionPaper
    {
        public string FileLink { get; set; }
        public string FileName { get; set; }
    }

    enum QPCategories { Semester, Year, QuestionPaper };
    #endregion

    // Offline File Related Function Profile
    #region Offline File Profile
    struct OfflineFile
    {
        public string FilePath { get; set; }
        public string FileExtension { get; set; }
    }
    #endregion

    // About Screen Structure
    #region About Screen
    struct About
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
    #endregion

    // Application Constants
    #region Application Constanst
    static class AppConstant
    {
        public static string AppFolderPath
        {
            get
            {
                return System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "UniHub");
            }
        }

        public static string QPFolderPath
        {
            get
            {
                return System.IO.Path.Combine(AppFolderPath, "Question Papers");
            }
        }

        public static string NoticePath
        {
            get
            {
                return System.IO.Path.Combine(AppFolderPath, "Notices");
            }
        }

        public static void SetUpDirectories()
        {
            if (!Directory.Exists(AppFolderPath))
                Directory.CreateDirectory(AppFolderPath);

            if (!Directory.Exists(QPFolderPath))
                Directory.CreateDirectory(QPFolderPath);

            if (!Directory.Exists(NoticePath))
                Directory.CreateDirectory(NoticePath);
        }

        public static List<OfflineFile> GetOfflineQuestionPapers()
        {
            List<OfflineFile> list = new List<OfflineFile>();
            list.Clear();

            string[] files = Directory.GetFiles(QPFolderPath, "*.pdf", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                OfflineFile of = new OfflineFile()
                {
                    FilePath = file,
                    FileExtension = System.IO.Path.GetExtension(file)
                };

                list.Add(of);
            }

            return list;
        }

        public static List<OfflineFile> GetOfflineNotices()
        {
            var ext = new List<string> { "jpg", "gif", "png" };

            List<OfflineFile> list = new List<OfflineFile>();
            list.Clear();

            List<string> files = Directory.GetFiles(NoticePath, "*.*", SearchOption.AllDirectories)
                                    .Where(file => new string[] { ".jpg", ".pdf", ".png", ".doc", ".docx" }
                                    .Contains(System.IO.Path.GetExtension(file)))
                                    .ToList<string>();

            foreach (string file in files)
            {
                OfflineFile of = new OfflineFile()
                {
                    FilePath = file,
                    FileExtension = System.IO.Path.GetExtension(file)
                };

                list.Add(of);
            }

            return list;
        }

        public static bool IsNoticeOffline(string filename)
        {
            if (File.Exists(System.IO.Path.Combine(NoticePath, filename)))
                return true;
            else
                return false;
        }

        public static bool IsQPOffline(string filename)
        {
            if (File.Exists(System.IO.Path.Combine(QPFolderPath, filename)))
                return true;
            else
                return false;
        }
    }
    #endregion

    // View Holder Profile For List View
    #region View Holder Profile
    public class NewsViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
        public TextView PublishDate { get; set; }
        public ImageView Image { get; set; }
    }

    public class PlacementsViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
        public TextView PublishDate { get; set; }
        public ImageView Image { get; set; }
    }

    public class NoticeViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
        public TextView Description { get; set; }
        public TextView Icon { get; set; }
        public LinearLayout BaseCover { get; set; }
        public LinearLayout BaseIcon { get; set; }
    }
    #endregion

    // Extended App Compat Activity Profiles
    #region Extented App Compat Activity
    public class ExtendedAppCompatActivity : AppCompatActivity
    {
        public enum ActivityState { Start, Resume, Pause, Stop, Restart };

        public ActivityState CurrentState { get; set; }

        protected override void OnResume()
        {
            base.OnResume();
            CurrentState = ActivityState.Resume;
        }

        protected override void OnPause()
        {
            base.OnPause();
            CurrentState = ActivityState.Pause;
        }

        protected override void OnStart()
        {
            base.OnStart();
            CurrentState = ActivityState.Start;
        }

        protected override void OnStop()
        {
            base.OnStop();
            CurrentState = ActivityState.Stop;
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            CurrentState = ActivityState.Restart;
        }
    }
    #endregion
}