namespace MosPolyHelper.Features.Main
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.Core.App;
    using AndroidX.Core.View;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.Preference;
    using Google.Android.Material.Navigation;
    using MosPolyHelper.Features.Addresses;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Common.Interfaces;
    using MosPolyHelper.Features.Schedule;
    using MosPolyHelper.Features.Settings;
    using MosPolyHelper.Features.Splash;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Threading.Tasks;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false,
    ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan)]
    public class MainView : AppCompatActivity, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        string clickBackAgain;
        bool doubleBackToExitPressedOnce;
        IFragmentBase prevFragment;
        IFragmentBase currFragment;
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
            this.viewModel = new MainVm(DependencyInjector.GetIMediator());
            StringProvider.SetUpLogger(this.loggerFactory);
            var awaiter = SplashView.ScheduleVmPreloadTask?.GetAwaiter();
            ScheduleVm scheduleVm = null;
            if (awaiter.HasValue)
            {
                scheduleVm = awaiter.Value.GetResult();
            }
            SplashView.ScheduleVmPreloadTask = null;
            if (savedInstanceState == null)
            {
                ChangeFragment(ScheduleView.NewInstance(scheduleVm), true);
            }
            else
            {
                // TODO: Fix stack
                //ChangeFragment(this.SupportFragmentManager.GetBackStackEntryAt(0)., Fragments.ScheduleMain, false);
            }

            ActivityCompat.RequestPermissions(this,
                new string[] { Android.Manifest.Permission.Internet }, 123);

            this.logger = this.loggerFactory.Create<MainView>();
            var currentNightMode = this.Resources.Configuration.UiMode;

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += (obj, arg) => OnNavigationItemSelected(arg.MenuItem);
            navigationView.SetCheckedItem(Resource.Id.nav_schedule);
            this.clickBackAgain = GetString(Resource.String.click_back_again);
            this.doubleBackToExitPressedOnce = false;
            PreferenceManager.GetDefaultSharedPreferences(this)
                .RegisterOnSharedPreferenceChangeListener(this);
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
            var settingsDrawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_schedule);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
                actionDone = true;
            }
            if (settingsDrawer != null && settingsDrawer.IsDrawerOpen(GravityCompat.End))
            {
                settingsDrawer.CloseDrawer(GravityCompat.End);
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

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case PreferencesConstants.ScheduleShowColoredLessons:
                    this.viewModel.ChangeShowEmptyLessons(sharedPreferences.GetBoolean(key, default));
                    break;
                case PreferencesConstants.ScheduleShowEmptyLessons:
                    this.viewModel.ChangeShowColoredLessons(sharedPreferences.GetBoolean(key, default));
                    break;
                case "NightMode":
                    AppCompatDelegate.DefaultNightMode = sharedPreferences.GetBoolean(key, default) ?
                        AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo;

                    this.Delegate.ApplyDayNight();
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

            Func<IFragmentBase> fragmentCreator = null;
            if (id == Resource.Id.nav_schedule)
            {
                fragmentId = Fragments.ScheduleMain;
                fragmentCreator = () => ScheduleView.NewInstance();
            }
            else if (id == Resource.Id.nav_buildings)
            {
                fragmentId = Fragments.Buildings;
                fragmentCreator = () => new AddressesView();
            }
            else if (id == Resource.Id.nav_settings)
            {
                fragmentId = Fragments.Settings;
                fragmentCreator = () => SettingsView.NewInstance();
            }
            if (this.currFragment?.FragmentType == fragmentId)
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
            ChangeFragment(fragmentCreator.Invoke(), true);
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

        public void ChangeFragment(IFragmentBase fragment, bool disposePrevious)
        {
            this.prevFragment = this.SupportFragmentManager.FindFragmentById(Resource.Id.frame_schedule) as FragmentBase;
            if (this.prevFragment != null && disposePrevious)
            {
                this.prevFragment.Fragment.Dispose();
            }
            if (disposePrevious)
            {
                this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frame_schedule, fragment.Fragment).Commit();
            }
            else
            {
                this.SupportFragmentManager.BeginTransaction().Add(Resource.Id.frame_schedule, fragment.Fragment)
                    .AddToBackStack(null).Commit();
            }
            this.currFragment = fragment;
        }

        protected override void OnDestroy()
        {
            NLog.LogManager.Flush();
            PreferenceManager.GetDefaultSharedPreferences(this)
                .UnregisterOnSharedPreferenceChangeListener(this);
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

