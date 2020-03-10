namespace MosPolyHelper.Features.Splash
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using AndroidX.AppCompat.App;
    using AndroidX.Preference;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Features.Schedule;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System.Threading.Tasks;

    [Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true,
    ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashView : AppCompatActivity
    {
        public static Task<ScheduleVm> ScheduleVmPreloadTask;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Environment.SetEnvironmentVariable("ScheduleVersion", "2");
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            AppCompatDelegate.DefaultNightMode = prefs.GetBoolean("NightMode", default) ?
                        AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo;
            StringProvider.Context = this;
            var loggerFactory = DependencyInjector.GetILoggerFactory(this.Assets.Open("NLog.config"));
            AssetProvider.AssetManager = this.Assets;
            StringProvider.SetUpLogger(loggerFactory);


            bool res = prefs.GetBoolean(PreferencesConstants.FirstLaunch, true);
            res = false;
            if (res)
            {
                base.OnCreate(savedInstanceState);
                prefs.Edit().PutBoolean(PreferencesConstants.FirstLaunch, false).Apply();

            }
            else
            {
                ScheduleVmPreloadTask = Task.Run(
                    () => PrepareSchdeuleVm(loggerFactory, prefs));
                base.OnCreate(savedInstanceState);
                StartActivity(new Intent(Application.Context, typeof(MainView)));
            }

        }

        protected override void OnStop()
        {
            base.OnStop();
            Finish();
        }

        ScheduleVm PrepareSchdeuleVm(ILoggerFactory loggerFactory, ISharedPreferences prefs)
        {
            string groupTitle = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, null);

            var scheduleFilter = Domains.ScheduleDomain.Schedule.Filter.DefaultFilter;
            scheduleFilter.DateFilter = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter,
                (int)scheduleFilter.DateFilter);
            scheduleFilter.SessionFilter = prefs.GetBoolean(PreferencesConstants.ScheduleSessionFilter,
                scheduleFilter.SessionFilter);

            bool isSession = prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0) == 1;

            var viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), isSession, scheduleFilter)
            {
                GroupTitle = groupTitle
            };
            viewModel.ShowEmptyLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowEmptyLessons, false);
            viewModel.ShowColoredLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowColoredLessons, true);
            if (groupTitle != null)
            {
                viewModel.SetUpScheduleAsync(false, true);
            }
            return viewModel;
        }
    }
}