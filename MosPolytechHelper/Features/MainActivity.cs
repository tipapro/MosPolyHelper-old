namespace MosPolytechHelper
{
    using Android.App;
    using Android.OS;
    using Android.Runtime;
    using Android.Support.Design.Widget;
    using Android.Support.V4.View;
    using Android.Support.V4.Widget;
    using Android.Support.V7.App;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using MosPolytechHelper.Features.Common;
    using MosPolytechHelper.Features.StudentSchedule;
    using System;
    using System.Threading.Tasks;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        bool doubleBackToExitPressedOnce;
        Fragments curFragmentId = Fragments.Other;
        Fragments prevFragmentId = Fragments.Other;
        FragmentBase prevFragment;
        ILoggerFactory loggerFactory;
        ILogger logger;
        PopupWindow popupPreferences;
        ScheduleFilterView popupFilter;

        ScheduleVm PrepareSchdeuleVm(ILoggerFactory loggerFactory)
        {
            var prefs = GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            string groupTitle = prefs.GetString("ScheduleGroupTitle", null);
            if (groupTitle == null)
            {
                return null;
            }

            var scheduleFilter = new Schedule.Filter();
            scheduleFilter.DateFitler = (DateFilter)prefs.GetInt("ScheduleDateFilter", (int)scheduleFilter.DateFitler);
            scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt("ScheduleModuleFilter", (int)scheduleFilter.ModuleFilter);
            scheduleFilter.SessionFilter = prefs.GetBoolean("ScheduleSessionFilter", scheduleFilter.SessionFilter);

            bool isSession = prefs.GetInt("ScheduleTypePreference", 0) == 1;

            var viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), isSession, scheduleFilter)
            {
                GroupTitle = groupTitle
            };
            viewModel.SetUpScheduleAsync(false);
            return viewModel;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            if (this.loggerFactory == null)
            {
                this.loggerFactory = DependencyInjector.GetILoggerFactory();
            }
            var scheduleVm = PrepareSchdeuleVm(this.loggerFactory);

            ChangeFragment(ScheduleView.NewInstance(scheduleVm), Fragments.ScheduleMain, false);

            Android.Support.V4.App.ActivityCompat.RequestPermissions(this,
                new string[] { Android.Manifest.Permission.Internet }, 123);

            Android.Support.V4.App.ActivityCompat.RequestPermissions(this,
                new string[] { Android.Manifest.Permission.Internet , Android.Manifest.Permission.WriteExternalStorage
                , Android.Manifest.Permission.ReadExternalStorage }, 1234);
            this.logger = this.loggerFactory.Create<MainActivity>();
            

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            this.doubleBackToExitPressedOnce = false;
            logger.Debug("MainActivity OnCreate Successful");
        }

        public override void OnBackPressed()
        {
            bool actionDone = false;
            if (this.popupPreferences != null && this.popupPreferences.IsShowing)
            {
                this.popupPreferences.Dismiss();
                actionDone = true;
            }
            if (this.popupFilter != null && this.popupFilter.IsShowing)
            {
                this.popupFilter.Dismiss();
                actionDone = true;
            }

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
                actionDone = true;
            }
            if (!actionDone)
            {
                if (this.doubleBackToExitPressedOnce)
                {
                    base.OnBackPressed();
                }
                else
                {
                    Toast.MakeText(this, "Please click BACK again to exit", ToastLength.Short).Show();
                    this.doubleBackToExitPressedOnce = true;
                }
                UpdateExitFlag();
            }
        }

        async void UpdateExitFlag()
        {
            await Task.Delay(2000);
            this.doubleBackToExitPressedOnce = false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.schedule_preferences)
            {
                if (this.popupPreferences == null)
                {
                    var inflater = LayoutInflater.From(this);
                    var layout = inflater.Inflate(Resource.Layout.popup_schedule_preferences, null);
                    this.popupPreferences = new SchedulePreferencesView(layout, LinearLayout.LayoutParams.WrapContent,
                        LinearLayout.LayoutParams.WrapContent, this.loggerFactory, DependencyInjector.GetIMediator())
                    {
                        //popupPreferences.ShowAtLocation(FindViewById<RelativeLayout>(Resource.Id.layout_main), GravityFlags.Top | GravityFlags.Right, 0, 0);
                        OutsideTouchable = true,
                        Focusable = true
                    };
                }
                if (!this.popupPreferences.IsShowing)
                {
                    this.popupPreferences.ShowAsDropDown(
                            FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar), 0, 0, GravityFlags.Right);
                }
                else
                {
                    this.popupPreferences.Dismiss();
                }

                return true;
            }
            else if (id == Resource.Id.schedule_filter)
            {
                if (this.popupFilter == null)
                {
                    var inflater = LayoutInflater.From(this);
                    var layout = inflater.Inflate(Resource.Layout.popup_schedule_filter, null);
                    this.popupFilter = new ScheduleFilterView(layout, LinearLayout.LayoutParams.WrapContent,
                        LinearLayout.LayoutParams.WrapContent, this.loggerFactory, DependencyInjector.GetIMediator())
                    {
                        OutsideTouchable = true,
                        Focusable = true
                    };
                }
                if (!this.popupFilter.IsShowing)
                {
                    float scale = this.Resources.DisplayMetrics.Density;
                    int padding12InPx = (int)(12 * scale + 0.5f);
                    this.popupFilter.ShowAsDropDown(
                            FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar), padding12InPx * 9, 0, GravityFlags.AxisXShift);
                }
                else
                {
                    this.popupFilter.Dismiss();
                }

                return true;
            }

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

        //public override bool OnSupportNavigateUp()
        //{
        //    ChangeFragment(this.prevFragment, this.prevFragment.FragmentType, true);
        //    return true;
        //}

        //Func<Android.Support.V4.App.Fragment> GetFragmentCreatorById(Fragments fragmentId)
        //{
        //    switch (fragmentId)
        //    {
        //        case Fragments.ScheduleMain:
        //            return () => ScheduleView.NewInstance();
        //        default:
        //            return null;
        //    }
        //}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1234 && (grantResults.Length == 0 || grantResults.Length == 2 && grantResults[0] == Android.Content.PM.Permission.Granted
                && grantResults[1] == Android.Content.PM.Permission.Granted))
            {
                if (this.loggerFactory == null)
                {
                    this.loggerFactory = DependencyInjector.GetILoggerFactory();
                }
                this.loggerFactory.CanWriteToFileChanged(true, 
                    GetExternalFilesDir(Android.OS.Environment.DataDirectory.AbsolutePath).AbsolutePath);
            }
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
            NLog.LogManager.Shutdown();
            base.OnDestroy();
        }
    }
}

