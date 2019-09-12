namespace MosPolytechHelper
{
    using Android.App;
    using Android.Graphics;
    using Android.Graphics.Drawables;
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
    using MosPolytechHelper.Features.Common;
    using MosPolytechHelper.Features.StudentSchedule;
    using System.Threading.Tasks;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, IMain
    {
        bool doubleBackToExitPressedOnce;
        ILoggerFactory loggerFactory;
        PopupWindow popupPreferences;
        ScheduleFilterView popupFilter;

        DependencyInjector IMain.DependencyInjector { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Android.Support.V4.App.ActivityCompat.RequestPermissions(this,
                new string[] { Android.Manifest.Permission.Internet }, 1234);

            DependencyInjector.SetDiInstance(this);
            this.loggerFactory = (this as IMain).DependencyInjector.GetILoggerFactory();

            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            this.SupportFragmentManager.BeginTransaction().
                Replace(Resource.Id.frame_schedule, ScheduleView.NewInstance()).Commit();

            doubleBackToExitPressedOnce = false;
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
                if (doubleBackToExitPressedOnce)
                {
                    base.OnBackPressed();
                }
                else
                {
                    Toast.MakeText(this, "Please click BACK again to exit", ToastLength.Short).Show();
                    doubleBackToExitPressedOnce = true;
                }
                UpdateExitFlag();
            }
        }

        async void UpdateExitFlag()
        {
            await Task.Delay(2000);
            doubleBackToExitPressedOnce = false;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
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
                    this.popupPreferences = new PopupWindow(layout, LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                    //popupPreferences.ShowAtLocation(FindViewById<RelativeLayout>(Resource.Id.layout_main), GravityFlags.Top | GravityFlags.Right, 0, 0);
                    this.popupPreferences.OutsideTouchable = true;
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
                        LinearLayout.LayoutParams.WrapContent, loggerFactory, (this as IMain).DependencyInjector.GetIMediator());
                    this.popupFilter.OutsideTouchable = true;
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

            Android.Support.V4.App.Fragment fragment = null;
            if (id == Resource.Id.nav_schedule)
            {
                fragment = ScheduleView.NewInstance();
            }
            this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frame_schedule, fragment).Commit();
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public ILoggerFactory GetILoggerFactory()
        {
            return loggerFactory;
        }
        public IMediator<ViewModels, VmMessage> GetIMediator()
        {
            return (this as IMain).DependencyInjector.GetIMediator();
        }
    }
}

