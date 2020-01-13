using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using MosPolyHelper.Common;
using MosPolyHelper.Common.Interfaces;
using MosPolyHelper.Domain;
using MosPolyHelper.Features.Main;
using MosPolyHelper.Features.Schedule;
using System.Threading.Tasks;
using static MosPolyHelper.Domain.Schedule;

namespace MosPolyHelper.Features
{
    [Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true,
    ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashView : AppCompatActivity
    {
        public static Task<ScheduleVm> ScheduleVmPreloadTask;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            AppCompatDelegate.DefaultNightMode = prefs.GetBoolean("NightMode", default) ?
                        AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo;
            StringProvider.Context = this;
            var loggerFactory = DependencyInjector.GetILoggerFactory(this.Assets.Open("NLog.config"));
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

            var scheduleFilter = Filter.DefaultFilter;
            scheduleFilter.DateFitler = (DateFilter)prefs.GetInt(PreferencesConstants.ScheduleDateFilter, 
                (int)scheduleFilter.DateFitler);
            //scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt(PreferencesConstants.ScheduleModuleFilter, (int)scheduleFilter.ModuleFilter);
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