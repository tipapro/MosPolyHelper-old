//namespace MosPolyHelper.Features.Splash
//{
//    using Android.App;
//    using Android.Content;
//    using Android.Content.PM;
//    using Android.OS;
//    using AndroidX.AppCompat.App;
//    using AndroidX.Preference;
//    using MosPolyHelper.Domains.ScheduleDomain;
//    using MosPolyHelper.Features.Main;
//    using MosPolyHelper.Features.Schedule;
//    using MosPolyHelper.Utilities;
//    using MosPolyHelper.Utilities.Interfaces;
//    using System.Diagnostics;
//    using System.Threading.Tasks;
//    using System.Timers;

//    //[Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true,
//    //ScreenOrientation = ScreenOrientation.Portrait)]
//    public class SplashView : AppCompatActivity
//    {
//        public static Stopwatch Stopwatch = new Stopwatch();
//       // public static Task<ScheduleVm> ScheduleVmPreloadTask;
        
//        protected override void OnCreate(Bundle savedInstanceState)
//        {
            

//            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
//            AppCompatDelegate.DefaultNightMode = prefs.GetBoolean("NightMode", default) ?
//                        AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo;

            
//            System.Environment.SetEnvironmentVariable("ScheduleVersion", "2");
//            StringProvider.Context = this;
//            AssetProvider.AssetManager = this.Assets;
//            bool res = prefs.GetBoolean(PreferencesConstants.FirstLaunch, true);
//            //var res = false;
//            if (res)
//            {
//                prefs.Edit().Clear().Apply();
//            }
//            //base.OnCreate(savedInstanceState);
//            //prefs.Edit().PutBoolean(PreferencesConstants.FirstLaunch, false).Apply();

//            //}
//            //else
//            //{
//            //ScheduleVmPreloadTask = Task.Run(
//             //   () => PrepareSchdeuleVm(prefs));
            
//            StartActivity(new Intent(Application.Context, typeof(MainView)));
//            //}
//            base.OnCreate(savedInstanceState);
//        }

//        protected override void OnStop()
//        {
//            base.OnStop();
//            Finish();
//        }

//        ScheduleVm PrepareSchdeuleVm(ISharedPreferences prefs)
//        {
//            var loggerFactory = DependencyInjector.GetILoggerFactory(this.Assets.Open("NLog.config"));
//            string groupTitle = prefs.GetString(PreferencesConstants.ScheduleGroupTitle, null);

//            var scheduleFilter = Schedule.Filter.DefaultFilter;
//            scheduleFilter.DateFilter = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter,
//                (int)scheduleFilter.DateFilter);
//            scheduleFilter.SessionFilter = prefs.GetBoolean(PreferencesConstants.ScheduleSessionFilter,
//                scheduleFilter.SessionFilter);

//#warning fix on release
//            bool isSession;
//            try
//            {
//                isSession = prefs.GetInt(PreferencesConstants.ScheduleTypePreference, 0) == 1;
//            }
//            catch
//            {
//                isSession = prefs.GetBoolean(PreferencesConstants.ScheduleTypePreference, false);
//            }

//            var viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), isSession, scheduleFilter)
//            {
//                GroupTitle = groupTitle
//            };
//            viewModel.ShowEmptyLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowEmptyLessons, false);
//            viewModel.ShowColoredLessons = prefs.GetBoolean(PreferencesConstants.ScheduleShowColoredLessons, true);
//            //viewModel.ScheduleFromPreferences(null);    // prefs.GetString(PreferencesConstants.Schedule, null));
//            if (groupTitle != null)
//            {
//                viewModel.SetUpScheduleAsync(false, true);
//            }
//            return viewModel;
//        }
//    }
//}