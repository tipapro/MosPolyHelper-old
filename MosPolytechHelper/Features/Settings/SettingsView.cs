namespace MosPolyHelper.Features.Settings
{
    using Android.OS;
    using Android.Views;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.Fragment.App;
    using AndroidX.Preference;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;

    class SettingsView : FragmentPreferenceBase, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback
    {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);
        }

        public SettingsView() : base(Fragments.Settings)
        {

        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);

            if (toolbar != null)
            {
                (this.Activity as MainView)?.SetSupportActionBar(toolbar);
            }
            if (this.PreferenceScreen.Key == "MainScreen")
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
                (this.Activity as MainView)?.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                (this.Activity as MainView)?.SupportActionBar.SetHomeButtonEnabled(true);
                var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            }
        }

        public override Fragment CallbackFragment
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
            args.PutString(ArgPreferenceRoot, pref.Key);
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