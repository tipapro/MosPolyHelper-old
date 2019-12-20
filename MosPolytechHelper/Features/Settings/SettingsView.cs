namespace MosPolyHelper.Features.Settings
{
    using Android.OS;
    using Android.Support.V4.Widget;
    using Android.Support.V7.App;
    using Android.Support.V7.Preferences;
    using Android.Views;

    class SettingsView : PreferenceFragmentCompat, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback
    {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            this.PreferenceScreen.PreferenceChange += (this.Activity as MainActivity).OnSharedPrefencesChanged;
            //var toolbarLayout = this.Activity.LayoutInflater.Inflate(Resource.Layout.toolbar_settings, null);
            //var toolbar = toolbarLayout.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolba);
            //(this.Activity as MainActivity).SetSupportActionBar(toolbar);

            //var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //var toggle = new ActionBarDrawerToggle(this.Activity, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            //drawer.AddDrawerListener(toggle);
            //toggle.SyncState();
            //toggle.DrawerIndicatorEnabled = true;
            //var appBarLayout = (this.Activity as MainActivity).FindViewById<AppBarLayout>(Resource.Id.appbar);
            //appBarLayout.RemoveAllViews();
            //appBarLayout.AddView(toolbarLayout);
        }
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        { 
            base.OnViewCreated(view, savedInstanceState);
            var toolbar = view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            if (toolbar != null)
            {
                (this.Activity as MainActivity)?.SetSupportActionBar(toolbar);
            }
            if (this.PreferenceScreen.Key == "a0")
            {
                var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                var toggle = new ActionBarDrawerToggle(this.Activity, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
                drawer.AddDrawerListener(toggle);
                toggle.SyncState();
                toggle.DrawerIndicatorEnabled = true;
                drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            }
            else
            {
                (this.Activity as MainActivity)?.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                (this.Activity as MainActivity)?.SupportActionBar.SetHomeButtonEnabled(true);
                var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);

            }

        }

        public override Android.Support.V4.App.Fragment CallbackFragment
        { 
            get 
            {
                return this;
            }
        }

        public bool OnPreferenceStartScreen(PreferenceFragmentCompat caller, PreferenceScreen pref)
        {
            var fragment = new SettingsView();
            var args = new Bundle();
            args.PutString(PreferenceFragmentCompat.ArgPreferenceRoot, pref.Key);
            fragment.Arguments = args;
            this.Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frame_schedule, fragment, pref.Key)
                .AddToBackStack(pref.Key).Commit();
            return true;
        }

        public static SettingsView NewInstance()
        {
            return new SettingsView();
        }
    }
}