using Android.Content;


namespace UniHub
{
    interface ISettings
    {
        string LastNoticeUpdate { get; set; }

        string LastPlacementUpdate { get; set; }

        string LastNewsUpdate { get; set; }

        bool IsFirstTimeLaunch { get; set; }
    }

    class Settings : ISettings
    {
        ISharedPreferences sharePref;
        ISharedPreferencesEditor editor;
        Context context;

        //shared preference values
        private const string PREFERENCE_NAME = "UniHubSettings";
        private const string LAST_NOTICE_UPDATE = "LastNoticeUpdate";
        private const string LAST_PLACEMENT_UPDATE = "LastPlacementUpdate";
        private const string LAST_NEWS_UPDATE = "LastNewsUpdate";
        private const string IS_FIRST_TIME_LAUNCH = "FirstTimeLaunch";

        public Settings(Context c)
        {
            this.context = c;
            sharePref = this.context.GetSharedPreferences(PREFERENCE_NAME, FileCreationMode.Private);
            editor = sharePref.Edit();
        }

        public string LastNewsUpdate
        {
            get
            {
                return sharePref.GetString(LAST_NEWS_UPDATE, null);
            }

            set
            {
                editor.PutString(LAST_NEWS_UPDATE, value);
                editor.Commit();
            }
        }

        public string LastNoticeUpdate
        {
            get
            {
                return sharePref.GetString(LAST_NOTICE_UPDATE, null);
            }

            set
            {
                editor.PutString(LAST_NOTICE_UPDATE, value);
                editor.Commit();
            }
        }

        public string LastPlacementUpdate
        {
            get
            {
                return sharePref.GetString(LAST_PLACEMENT_UPDATE, null);
            }

            set
            {
                editor.PutString(LAST_PLACEMENT_UPDATE, value);
                editor.Commit();
            }
        }

        public bool IsFirstTimeLaunch
        {
            get
            {
                return sharePref.GetBoolean(IS_FIRST_TIME_LAUNCH, true);
            }

            set
            {
                editor.PutBoolean(LAST_NOTICE_UPDATE, value);
                editor.Commit();
            }
        }
    }
}