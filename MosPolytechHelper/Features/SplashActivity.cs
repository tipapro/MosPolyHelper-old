using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using MosPolyHelper.Common;
using MosPolyHelper.Common.Interfaces;
using MosPolyHelper.Domain;
using MosPolyHelper.Features.Schedule;
using System;
using System.Threading.Tasks;
using static MosPolyHelper.Domain.Schedule;

namespace MosPolyHelper.Features
{
    [Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true,
    ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        public static Task<ScheduleVm> ScheduleVmPreloadTask;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            StringProvider.Context = this;
            ScheduleVmPreloadTask = Task.Run(() => PrepareSchdeuleVm(DependencyInjector.GetILoggerFactory(Assets.Open("NLog.config"))));
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        protected override void OnStop()
        {
            base.OnStop();
            Finish();
        }

        ScheduleVm PrepareSchdeuleVm(ILoggerFactory loggerFactory)
        {
            StringProvider.SetUpLogger(loggerFactory);
            var prefs = GetSharedPreferences("SchedulePreferences", Android.Content.FileCreationMode.Private);
            string groupTitle = prefs.GetString("ScheduleGroupTitle", null);
            if (groupTitle == null)
            {
                return null;
            }

            var scheduleFilter = new Filter();
            scheduleFilter.DateFitler = (DateFilter)prefs.GetInt("ScheduleDateFilter", (int)scheduleFilter.DateFitler);
            scheduleFilter.ModuleFilter = (ModuleFilter)prefs.GetInt("ScheduleModuleFilter", (int)scheduleFilter.ModuleFilter);
            scheduleFilter.SessionFilter = prefs.GetBoolean("ScheduleSessionFilter", scheduleFilter.SessionFilter);

            bool isSession = prefs.GetInt("ScheduleTypePreference", 0) == 1;

            var viewModel = new ScheduleVm(loggerFactory, DependencyInjector.GetIMediator(), isSession, scheduleFilter)
            {
                GroupTitle = groupTitle
            };
            viewModel.ShowEmptyLessons = prefs.GetBoolean("ScheduleShowEmptyLessons", false);
            viewModel.ShowColoredLessons = prefs.GetBoolean("ScheduleShowColoredLessons", false);
            viewModel.SetUpScheduleAsync(false, true);
            return viewModel;
        }
    }
}