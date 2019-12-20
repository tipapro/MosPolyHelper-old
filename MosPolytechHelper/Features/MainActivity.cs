namespace MosPolyHelper.Features
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Runtime;
    using Android.Support.Design.Widget;
    using Android.Support.V4.View;
    using Android.Support.V4.Widget;
    using Android.Support.V7.App;
    using Android.Support.V7.Preferences;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Schedule;
    using MosPolyHelper.Features.Settings;
    using System;
    using System.Threading.Tasks;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false,
    ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        string clickBackAgain;
        bool doubleBackToExitPressedOnce;
        Fragments curFragmentId = Fragments.Other;
        Fragments prevFragmentId = Fragments.Other;
        FragmentBase prevFragment;
        ILoggerFactory loggerFactory;
        ILogger logger;
        MainVm viewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            if (this.loggerFactory == null)
            {
                this.loggerFactory = DependencyInjector.GetILoggerFactory();
            }
            viewModel = new MainVm(DependencyInjector.GetIMediator());
            StringProvider.SetUpLogger(loggerFactory);
            var awaiter = SplashActivity.ScheduleVmPreloadTask?.GetAwaiter();
            ScheduleVm scheduleVm = null;
            if (awaiter.HasValue)
            {
                scheduleVm = awaiter.Value.GetResult();
            }
            SplashActivity.ScheduleVmPreloadTask = null;
            ChangeFragment(ScheduleView.NewInstance(scheduleVm), Fragments.ScheduleMain, false);

            Android.Support.V4.App.ActivityCompat.RequestPermissions(this,
                new string[] { Android.Manifest.Permission.Internet }, 123);

            this.logger = this.loggerFactory.Create<MainActivity>();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += (obj, arg) => OnNavigationItemSelected(arg.MenuItem);
            navigationView.SetCheckedItem(Resource.Id.nav_schedule);
            this.clickBackAgain = GetString(Resource.String.click_back_again);
            this.doubleBackToExitPressedOnce = false;
            this.logger.Debug("MainActivity created successfully");
        }

        protected override void OnResume()
        {
            base.OnResume();
            StringProvider.Context = this;
        }

        public PopupWindow CurrentPopupWindow { get; set; }

        public override void OnBackPressed()
        {
            bool actionDone = false;
            if (this.CurrentPopupWindow != null && this.CurrentPopupWindow.IsShowing)
            {
                this.CurrentPopupWindow.Dismiss();
                actionDone = true;
            }

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
                actionDone = true;
            }

            if (this.SupportFragmentManager.BackStackEntryCount != 0)
            {
                base.OnBackPressed();
                actionDone = true;
            }

            if (!actionDone)
            {
                if (this.doubleBackToExitPressedOnce)
                {
                    base.OnBackPressed();
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, this.clickBackAgain, ToastLength.Short).Show();
                    this.doubleBackToExitPressedOnce = true;
                }
                UpdateExitFlag();
            }
        }

        public void OnSharedPrefencesChanged(object sender, Preference.PreferenceChangeEventArgs e)
        {
            if (e.Preference.HasKey)
            {
                return;
            }
            switch (e.Preference.Key)
            {
                case PreferencesConstants.ScheduleShowColoredLessons:
                    this.viewModel.ChangeShowEmptyLessons((bool)e.NewValue);
                    break;
                case PreferencesConstants.ScheduleShowEmptyLessons:
                    this.viewModel.ChangeShowColoredLessons((bool)e.NewValue);
                    break;
            }
        }

        async void UpdateExitFlag()
        {
            await Task.Delay(2000);
            this.doubleBackToExitPressedOnce = false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            var fragmentId = Fragments.Other;
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            Func<Android.Support.V4.App.Fragment> fragmentCreator = null;
            if (id == Resource.Id.nav_schedule)
            {
                fragmentId = Fragments.ScheduleMain;
                fragmentCreator = () => ScheduleView.NewInstance();
            }
            else if (id == Resource.Id.nav_settings)
            {
                fragmentId = Fragments.Settings;
                fragmentCreator = () => SettingsView.NewInstance();
            }
            if (this.curFragmentId == fragmentId)
            {
                drawer.CloseDrawer(GravityCompat.Start);
                return false;
            }
            if (fragmentId == Fragments.Other)
            {
                this.logger.Warn("{ItemId} doesn't have own Fragments enum id", item.ItemId);
            }
            if (fragmentCreator == null)
            {
                drawer.CloseDrawer(GravityCompat.Start);
                return false;
            }
            ChangeFragment(fragmentCreator.Invoke(), fragmentId, true);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //if (requestCode == 1234 && (grantResults.Length == 0 || grantResults.Length == 2 && grantResults[0] == Android.Content.PM.Permission.Granted
            //    && grantResults[1] == Android.Content.PM.Permission.Granted))
            //{
            //    if (this.loggerFactory == null)
            //    {
            //        this.loggerFactory = DependencyInjector.GetILoggerFactory();
            //    }
            //}
        }

        public void ChangeFragment(Android.Support.V4.App.Fragment fragment, Fragments fragmentId, bool disposePrevious)
        {
            this.prevFragment = this.SupportFragmentManager.FindFragmentById(Resource.Id.frame_schedule) as FragmentBase;
            if (this.prevFragment != null && disposePrevious)
            {
                this.prevFragment.Dispose();
            }
            this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frame_schedule, fragment).Commit();
            this.prevFragmentId = this.curFragmentId;
            this.curFragmentId = fragmentId;
        }

        protected override void OnDestroy()
        {
            NLog.LogManager.Flush();
            base.OnDestroy();
        }

        protected override void OnStop()
        {
            NLog.LogManager.Flush();
            base.OnStop();
        }

        public override bool OnSupportNavigateUp()
        {
            base.OnBackPressed();
            return true;
        }
    }
}

