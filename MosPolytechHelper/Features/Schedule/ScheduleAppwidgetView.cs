namespace MosPolyHelper.Features.Schedule
{
    using Android.App;
    using Android.Appwidget;
    using Android.Content;
    using Android.Graphics;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Utilities;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    [BroadcastReceiver(Label = "@string/app_name")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/schedule_appwidget_provider")]
    public class ScheduleAppwidgetView : AppWidgetProvider
    {
        const string OpenApp = "OpenApp";
        const string UpdateSchedule = "UpdateSchedule";
        CultureInfo customFormat;
        /// <summary>
        /// This method is called when the 'updatePeriodMillis' from the AppwidgetProvider passes,
        /// or the user manually refreshes/resizes.
        /// </summary>
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {

            foreach (var id in appWidgetIds)
            {
                var rv = new RemoteViews(context.PackageName, Resource.Layout.appwidget_schedule);
                SetTextViewText(rv);
                var intent = new Intent(context, typeof(StackWidgetService));
                intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, id);
                rv.SetRemoteAdapter(Resource.Id.list_schedule, intent);


                var intent2 = new Intent(context, typeof(ScheduleAppwidgetView));
                intent2.SetAction(OpenApp);
                var piBackground = PendingIntent.GetBroadcast(context, 0, intent2, 0);
                rv.SetOnClickPendingIntent(Resource.Id.layout_schedule_widget, piBackground);


                var intent3 = new Intent(context, typeof(ScheduleAppwidgetView));
                var piBackground2 = PendingIntent.GetBroadcast(context, 0, intent3, 0);
                intent3.SetAction(UpdateSchedule);
                rv.SetOnClickPendingIntent(Resource.Id.button_schedule_refresh, piBackground2);

                appWidgetManager.UpdateAppWidget(id, rv);
            }
        }

        public ScheduleAppwidgetView() : base()
        {

            this.customFormat = CultureInfo.CurrentUICulture;
            this.customFormat = (CultureInfo)this.customFormat.Clone();
            this.customFormat.DateTimeFormat.AbbreviatedMonthNames =
                (from m in this.customFormat.DateTimeFormat.AbbreviatedMonthNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();

            this.customFormat.DateTimeFormat.AbbreviatedMonthGenitiveNames =
                (from m in this.customFormat.DateTimeFormat.AbbreviatedMonthGenitiveNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();

            this.customFormat.DateTimeFormat.AbbreviatedDayNames =
                (from m in this.customFormat.DateTimeFormat.AbbreviatedDayNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();
        }


        private void SetTextViewText(RemoteViews widgetView)
        {
            widgetView.SetTextViewText(Resource.Id.text_schedule_date,
                DateTime.Today.ToString("ddd, dd/MM", this.customFormat));
            //widgetView.SetTextViewText(Resource.Id.widgetSmall, string.Format("Last update: {0:H:mm:ss}", DateTime.Now));
        }

        //private PendingIntent GetPendingSelfIntent(Context context, string action)
        //{
        //	var intent = new Intent(context, typeof(ScheduleAppwidgetView));
        //	intent.SetAction(action);
        //	return PendingIntent.GetBroadcast(context, 0, intent, 0);
        //}

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            Toast.MakeText(context, "Clicked " + intent.Action, ToastLength.Short).Show();

            switch (intent.Action)
            {
                case OpenApp:
                    context.StartActivity(new Intent(context, typeof(Main.MainView)));
                    break;
            }


            // Check if the click is from the "Announcement" button
            //if (AnnouncementClick.Equals(intent.Action))
            //{
            //	var pm = context.PackageManager;
            //	try
            //	{
            //		var packageName = "com.android.settings";
            //		var launchIntent = pm.GetLaunchIntentForPackage(packageName);
            //		context.StartActivity(launchIntent);
            //	}
            //	catch
            //	{
            //		// Something went wrong :)
            //	}
            //}
        }
    }
    [Service(Permission = "android.permission.BIND_REMOTEVIEWS")]
    public class StackWidgetService : RemoteViewsService
    {
        public override IRemoteViewsFactory OnGetViewFactory(Intent intent)
        {
            return new StackRemoteViewsFactory(this.ApplicationContext, intent);
        }
    }
    class StackRemoteViewsFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
    {

        Domains.ScheduleDomain.Schedule.Daily dailySchedule;

        Context context;
        bool nightMode = false;
        DateTime lastUpdate = DateTime.MinValue;
        Schedule schedule;
        int appWidgetId;
        ScheduleAppwidgetModel model;
        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(235, 65, 65),     // Exam, Credit,..
            new Color(41, 182, 246)   // Other
        };
        readonly Color disabledColor = new Color(224, 224, 224);
        readonly Color headColor = new Color(85, 85, 85);
        readonly Color headCurrentColor = new Color(134, 134, 134);


        public int Count => this.dailySchedule?.Count ?? 0;

        bool RemoteViewsService.IRemoteViewsFactory.HasStableIds => true;

        // You can create a custom loading view (for instance when getViewAt() is slow.) If you
        // return null here, you will get the default loading view.
        public RemoteViews LoadingView => null;

        public int ViewTypeCount => 1;

        public StackRemoteViewsFactory(Context context, Intent intent)
        {
            this.context = context;
            this.appWidgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId,
                    AppWidgetManager.InvalidAppwidgetId);
        }
        public void OnCreate()
        {
            // In onCreate() you setup any connections / cursors to your data source. Heavy lifting,
            // for example downloading or creating content etc, should be deferred to onDataSetChanged()
            // or getViewAt(). Taking more than 20 seconds in this call will result in an ANR.
            //
            //if (schedule != null)
            //{
            //    
            //}
            // We sleep for 3 seconds here to show how the empty view appears in the interim.
            // The empty view is set in the StackWidgetProvider and should be a sibling of the
            // collection view.
            this.model = new ScheduleAppwidgetModel(DependencyInjector.GetILoggerFactory());
            try
            {
                //Thread.Sleep(3000);
            }
            catch (Java.Lang.InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }
        public void OnDestroy()
        {
            // In onDestroy() you should tear down anything that was setup for your data source,
            // eg. cursors, connections, etc.
        }

        public RemoteViews GetViewAt(int position)
        {
            var lesson = this.dailySchedule[position];

            //Toast.MakeText(context, "GetView" + lesson.Title, ToastLength.Long);

            var rv = new RemoteViews(this.context.PackageName, Resource.Layout.item_schedule_appwidget);
            var fixedPos = position;
            SetHead(rv, lesson, fixedPos != 0
                && this.dailySchedule[fixedPos - 1].EqualsTime(lesson, DateTime.Today));
            SetLessonType(rv, lesson);
            SetAuditoriums(rv, lesson);
            SetTitle(rv, lesson);
            SetTeachers(rv, lesson);

            // Next, we set a fill-intent which will be used to fill-in the pending intent template
            // which is set on the collection view in StackWidgetProvider.
            //Bundle extras = new Bundle();
            //extras.PutInt("sgdsgsgsd", position);
            //Intent fillInIntent = new Intent();
            //fillInIntent.PutExtras(extras);
            //rv.SetOnClickFillInIntent(Resource.Id.text_schedule_time_grid, fillInIntent);
            // You can do heaving lifting in here, synchronously. For example, if you need to
            // process an image, fetch something from the network, etc., it is ok to do it here,
            // synchronously. A loading view will show up in lieu of the actual contents in the
            // interim.
            try
            {
                //System.out.println("Loading view " + position);
                //Thread.Sleep(500);
            }
            catch (Java.Lang.InterruptedException e)
            {
                e.PrintStackTrace();
            }
            // Return the remote views object.
            return rv;
        }
        public long GetItemId(int position)
        {
            return position;
        }
        public void OnDataSetChanged()
        {
            if ((DateTime.Now - lastUpdate).Days >= 1)
            {
                schedule = this.model.GetScheduleAsync("191-721", false, true).GetAwaiter().GetResult();
                //Toast.MakeText(context, "Download new", ToastLength.Long);
                lastUpdate = DateTime.Now;
            }
            this.dailySchedule = schedule?.GetSchedule(DateTime.Today, Domains.ScheduleDomain.Schedule.Filter.DefaultFilter);
            // This is triggered when you call AppWidgetManager notifyAppWidgetViewDataChanged
            // on the collection view corresponding to this factory. You can do heaving lifting in
            // here, synchronously. For example, if you need to process an image, fetch something
            // from the network, etc., it is ok to do it here, synchronously. The widget will remain
            // in its current state while work is being done here, so you don't need to worry about
            // locking up the widget.
        }

        void SetHead(RemoteViews rv, Lesson lesson, bool prevEqual)
        {
            if (prevEqual)
            {
                //viewHolder.LessonOrder.Visibility = ViewStates.Gone;
                //viewHolder.LessonTime.Visibility = ViewStates.Gone;
                //viewHolder.Divider.Visibility = ViewStates.Visible;
                //viewHolder.Indicator.Visibility = ViewStates.Gone;
                rv.SetInt(Resource.Id.layout_schedule, "setBackgroundResource", Resource.Color.mtrl_btn_transparent_bg_color);
            }
            else
            {
                //if (this.currLessonOrder == lesson.Order)
                //{
                //    viewHolder.Indicator.Visibility = ViewStates.Visible;
                //    viewHolder.LessonTime.SetTextColor(this.headCurrentColor);
                //}
                //else
                //{
                //    viewHolder.Indicator.Visibility = ViewStates.Gone;
                //    viewHolder.LessonTime.SetTextColor(this.headColor);
                //}

                //viewHolder.LessonOrder.Visibility = ViewStates.Visible;
                //viewHolder.LessonTime.Visibility = ViewStates.Visible;
                //viewHolder.Divider.Visibility = ViewStates.Visible;
                rv.SetInt(Resource.Id.layout_schedule, "setBackgroundResource", Resource.Drawable.top_stroke);
                var (StartTime, EndTime) = lesson.GetTime();
                rv.SetTextViewText(Resource.Id.text_schedule_time, StartTime + " - " + EndTime);
                rv.SetTextViewText(Resource.Id.text_schedule_order, "#" + (lesson.Order + 1));
            }

        }

        void SetLessonType(RemoteViews rv, Lesson lesson, bool enabled = true)
        {
            rv.SetTextColor(Resource.Id.text_schedule_type, enabled ?
                (lesson.IsImportant() ? this.lessonTypeColors[0] : this.lessonTypeColors[1]) : this.disabledColor);
            rv.SetTextViewText(Resource.Id.text_schedule_type, lesson.Type.ToUpper());
            //viewHolder.LessonType.Enabled = enabled;
        }

        void SetAuditoriums(RemoteViews rv, Lesson lesson, bool enabled = true)
        {
            using var auditoriums = new SpannableStringBuilder();
            //viewHolder.LessonAuditoriums.Enabled = enabled;
            if (lesson.Auditoriums == null)
            {
                rv.SetTextViewText(Resource.Id.text_schedule_auditoriums, string.Empty);
                return;
            }
            if (enabled)
            {
                for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                {
                    var audTitle = Html.FromHtml(lesson.Auditoriums[i].Name.ToLower(), FromHtmlOptions.ModeLegacy);
                    if (!string.IsNullOrEmpty(lesson.Auditoriums[i].Color))
                    {
                        string colorString = lesson.Auditoriums[i].Color;
                        if (colorString.Length == 4)
                        {
                            colorString = "#" +
                                colorString[1] + colorString[1] +
                                colorString[2] + colorString[2] +
                                colorString[3] + colorString[3];
                        }
                        var color = Color.ParseColor(colorString);
                        if (this.nightMode)
                        {
                            float hue = color.GetHue();
                            if (hue > 214f && hue < 286f)
                            {
                                if (hue >= 250f)
                                {
                                    hue = 214f;
                                }
                                else
                                {
                                    hue = 286f;
                                }
                            }
                            color = Color.HSVToColor(
                                new float[] { hue, color.GetSaturation(), color.GetBrightness() * 3 });
                        }
                        auditoriums.Append(audTitle + ", ",
                            new ForegroundColorSpan(color),
                            SpanTypes.ExclusiveExclusive);
                    }
                    else
                    {
                        auditoriums.Append(audTitle + ", ");
                    }
                }
                if (lesson.Auditoriums.Length != 0)
                {
                    var audTitle = Html.FromHtml(lesson.Auditoriums[^1].Name.ToLower(), FromHtmlOptions.ModeLegacy);
                    if (!string.IsNullOrEmpty(lesson.Auditoriums[^1].Color))
                    {
                        string colorString = lesson.Auditoriums[^1].Color;
                        if (colorString.Length == 4)
                        {
                            colorString = "#" +
                                colorString[1] + colorString[1] +
                                colorString[2] + colorString[2] +
                                colorString[3] + colorString[3];
                        }
                        var color = Color.ParseColor(colorString);
                        if (this.nightMode)
                        {
                            float hue = color.GetHue();
                            if (hue > 214f && hue < 286f)
                            {
                                if (hue >= 250f)
                                {
                                    hue = 214f;
                                }
                                else
                                {
                                    hue = 286f;
                                }
                            }
                            color = Color.HSVToColor(
                                new float[] { hue, color.GetSaturation(), color.GetBrightness() * 3 });
                        }
                        auditoriums.Append(audTitle,
                            new ForegroundColorSpan(color),
                            SpanTypes.ExclusiveExclusive);
                    }
                    else
                    {
                        auditoriums.Append(audTitle);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                {
                    var audTitle = Html.FromHtml(lesson.Auditoriums[i].Name.ToLower(), FromHtmlOptions.ModeLegacy);
                    auditoriums.Append(audTitle + ", ",
                        new ForegroundColorSpan(this.disabledColor), SpanTypes.ExclusiveExclusive);
                }
                if (lesson.Auditoriums.Length != 0)
                {
                    var audTitle = Html.FromHtml(lesson.Auditoriums[^1].Name.ToLower(), FromHtmlOptions.ModeLegacy);
                    auditoriums.Append(audTitle,
                        new ForegroundColorSpan(this.disabledColor),
                        SpanTypes.ExclusiveExclusive);
                }
            }
            rv.SetTextViewText(Resource.Id.text_schedule_auditoriums, auditoriums);
        }

        void SetTitle(RemoteViews rv, Lesson lesson, bool enabled = true)
        {
            //viewHolder.LessonTitle.SetTextColor(enabled ? new Color(95, 107, 117) : new Color(215, 215, 215));
            rv.SetTextViewText(Resource.Id.text_schedule_title, lesson.Title);
            //viewHolder.LessonTitle.Enabled = enabled;
        }

        void SetTeachers(RemoteViews rv, Lesson lesson, bool enabled = true)
        {
            string teachers = string.Join(", ", lesson.GetShortTeacherNames());
            if (string.IsNullOrEmpty(teachers))
            {
                rv.SetViewVisibility(Resource.Id.text_schedule_teachers, ViewStates.Gone);
            }
            else
            {
                rv.SetTextViewText(Resource.Id.text_schedule_teachers, teachers);
                //viewHolder.LessonTeachers.Enabled = enabled;
                rv.SetViewVisibility(Resource.Id.text_schedule_teachers, ViewStates.Visible);
            }
        }
    }




}

namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    class ScheduleAppwidgetModel
    {
        const string CurrentExtension = ".current";
        const string OldExtension = ".backup";
        const string CustomExtension = ".custom";
        const string ScheduleFolder = "cached_schedules";
        const string SessionScheduleFolder = "session";
        const string RegularScheduleFolder = "regular";

        readonly ILogger logger;
        readonly IScheduleDownloader downloader;
        readonly IScheduleConverter scheduleConverter;
        readonly ISerializer serializer;
        readonly IDeserializer deserializer;

        int scheduleCounter;

        (Stream SerSchedule, long Time) OpenReadSchedule(string groupTitle, bool isSession)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ScheduleFolder, groupTitle, isSession ? SessionScheduleFolder : RegularScheduleFolder);
            if (!Directory.Exists(folder))
            {
                return (null, 0);
            }
            var files = Directory.GetFiles(folder).Select(Path.GetFileName);
            string fileToRead = null;
            string fileToReadOld = null;
            foreach (string fileName in files)
            {
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension == CurrentExtension)
                {
                    fileToRead = fileName;
                }
                else if (fileExtension == OldExtension)
                {
                    fileToReadOld = fileName;
                }
            }
            if (fileToRead == null)
            {
                if (fileToReadOld == null)
                {
                    return (null, 0);
                }
                fileToRead = fileToReadOld;
            }
            string strArr = Path.GetFileNameWithoutExtension(fileToRead);
            if (strArr == null)
            {
                return (null, 0);
            }
            long day = long.Parse(strArr);
            var serSchedule = File.OpenRead(Path.Combine(folder, fileToRead));
            return (serSchedule, day);
        }

        async Task<Domains.ScheduleDomain.Schedule> DownloadScheduleAsync(string groupTitle, bool isSession)
        {
            string serSchedule = await this.downloader.DownloadSchedule(groupTitle, isSession);
            var schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule, Announce);
            if (schedule != null)
            {
                schedule.IsSession = isSession;
            }
            return schedule;
        }

        async Task<Domains.ScheduleDomain.Schedule> DownloadScheduleAsync(string groupTitle, bool isSession, CancellationToken ct)
        {
            string serSchedule;
            try
            {
                serSchedule = await this.downloader.DownloadSchedule(groupTitle, isSession);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw ex;
                }
                return null;
            }
            ct.ThrowIfCancellationRequested();
            var schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule, Announce);
            if (schedule != null)
            {
                schedule.IsSession = isSession;
            }
            return schedule;
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string serGroupList = await this.downloader.DownloadGroupListAsync();
            return await this.scheduleConverter.ConvertToGroupList(serGroupList);
        }

        readonly object key = new object();

        public event Action<string> Announce;
        public event Action<int> DownloadProgressChanged;

        public Domains.ScheduleDomain.Schedule Schedule { get; private set; }
        public string SerializedSchedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleAppwidgetModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new ScheduleDownloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
            this.serializer = DependencyInjector.GetProtofubISerializer();
            this.deserializer = DependencyInjector.GetProtofubIDeserializer();
        }

        public async Task<Domains.ScheduleDomain.Schedule> GetScheduleAsync(string group, bool isSession, bool downloadNew)
        {
            if (downloadNew)
            {
                try
                {
                    this.Schedule = await DownloadScheduleAsync(group, isSession);
                    if (this.Schedule == null)
                    {
                        Announce?.Invoke(StringProvider.GetString(StringId.ScheduleWasntFounded));
                    }
                }
                catch (Exception ex1)
                {
                    this.logger.Error(ex1, "Download schedule error");
                    try
                    {
                        Announce?.Invoke(StringProvider.GetString(StringId.ScheduleWasntFounded));
                        var (serSchedule, time) = OpenReadSchedule(group, isSession);
                        this.Schedule = await this.deserializer.DeserializeAsync<Domains.ScheduleDomain.Schedule>(serSchedule);
                        if (this.Schedule == null)
                        {
                            throw new Exception("Read schedule from storage fail");
                        }
                        if (this.Schedule.Version != Domains.ScheduleDomain.Schedule.RequiredVersion)
                        {
                            throw new Exception("Read schedule from storage fail");
                        }
                        this.Schedule.LastUpdate = new DateTime(time);
                        this.Schedule.IsSession = isSession;
                        this.Schedule.SetUpGroup();
                        Announce.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasFounded));
                    }
                    catch (Exception ex2)
                    {
                        this.logger.Error(ex2, "Read schedule after download failed error");
                        Announce?.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasntFounded));
                        this.Schedule = null;
                    }
                }
            }
            else
            {
                if (this.Schedule != null && this.Schedule.Group.Title == group && this.Schedule.IsSession == isSession)
                {
                    return this.Schedule;
                }
                try
                {
                    var (serSchedule, time) = OpenReadSchedule(group, isSession);
                    this.Schedule = await this.deserializer.DeserializeAsync<Domains.ScheduleDomain.Schedule>(serSchedule);
                    if (this.Schedule == null)
                    {
                        throw new Exception("Read schedule from storage fail");
                    }
                    if (this.Schedule.Version != Domains.ScheduleDomain.Schedule.RequiredVersion)
                    {
                        throw new Exception("Read schedule from storage fail");
                    }
                    this.Schedule.LastUpdate = new DateTime(time);
                    this.Schedule.IsSession = isSession;
                    this.Schedule.SetUpGroup();
                }
                catch (Exception ex1)
                {
                    this.logger.Error(ex1, "Read schedule error");
                    Announce?.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasntFounded));
                    this.Schedule = null;
                }
            }

            if (this.Schedule == null)
            {
                return null;
            }
            if (group != this.Schedule?.Group?.Title)
            {
                this.logger.Warn("{group} != {scheduleGroupTitle}", group, this.Schedule?.Group?.Title);
            }
            return this.Schedule;
        }
    }
}