namespace MosPolyHelper.Adapters
{
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using AndroidX.ViewPager.Widget;
    using Java.Lang;
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;
    using System.Globalization;
    using System.Linq;
    using Object = Java.Lang.Object;

    public class DailyShedulePageAdapter : PagerAdapter
    {
        readonly View[] views;
        readonly TextView[] textViews;
        readonly PairAdapter[] recyclerAdapters;
        readonly RecyclerView[] recyclerViews;
        public Schedule Schedule;
        Schedule.Filter scheduleFilter;
        bool showEmptyLessons;
        bool showColoredLessons;
        bool showGroup;
        int count;
        CultureInfo customFormat;

        void SetFirstPosDate(Schedule schedule)
        {
            if (!schedule.IsByDate)
            {
                this.FirstPosDate = this.count == 400 ? DateTime.Today.AddDays(-200) : schedule.From;
            }
            else if (this.Schedule == null)
            {
                this.FirstPosDate = DateTime.Today;
            }
            else
            {
                this.FirstPosDate = new DateTime(this.Schedule.GetSchedule(0).Day);
            }
        }

        public DateTime FirstPosDate { get; private set; }
        public event Action<Lesson, DateTime> LessonClick;
        public override int Count => this.count;

        public DailyShedulePageAdapter(Schedule schedule, Schedule.Filter scheduleFilter,
            bool showEmptyLessons, bool showColoredLessons, bool showGroup)
        {
            this.Schedule = schedule;
            this.scheduleFilter = scheduleFilter;
            this.showEmptyLessons = showEmptyLessons;
            this.showColoredLessons = showColoredLessons;
            this.showGroup = showGroup;
            SetCount(schedule);
            if (this.Schedule != null)
            {
                SetFirstPosDate(this.Schedule);
            }
            this.recyclerAdapters = new PairAdapter[3];
            this.views = new View[3];
            this.textViews = new TextView[3];
            this.recyclerViews = new RecyclerView[3];

            this.customFormat = CultureInfo.CurrentUICulture;
            this.customFormat = (CultureInfo)this.customFormat.Clone();
            this.customFormat.DateTimeFormat.MonthNames =
                (from m in this.customFormat.DateTimeFormat.MonthNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();

            this.customFormat.DateTimeFormat.MonthGenitiveNames =
                (from m in this.customFormat.DateTimeFormat.MonthGenitiveNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();

            this.customFormat.DateTimeFormat.DayNames =
                (from m in this.customFormat.DateTimeFormat.DayNames
                 select m.Length == 0 || m.Length == 1 ? m : char.ToUpper(m[0]) + m.Substring(1))
                    .ToArray();
        }

        public void SetCount(Schedule schedule)
        {
            if (schedule == null)
            {
                this.count = 1;
            }
            else if (schedule.IsByDate)
            {
                this.count = TimeSpan.FromTicks(System.Math.Abs(
                    schedule.GetSchedule(0).Day - schedule.GetSchedule(schedule.Count - 1).Day)).Days + 1;
            }
            else
            {
                this.count = (schedule.To - schedule.From).Days + 1;
                if (this.count > 400)
                {
                    this.count = 400;
                }
            }
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            if (this.Schedule == null)
            {
                return new Java.Lang.String("Нет расписания");
            }
            if (this.Schedule.IsByDate)
            {
                return new Java.Lang.String(new DateTime(this.Schedule.GetSchedule(0).Day)
                    .AddDays(position).ToString(" ddd d MMM ").Replace('.', '\0').ToUpper());
            }
            else
            {
                return new Java.Lang.String(this.FirstPosDate
                    .AddDays(position).ToString(" ddd d MMM ").Replace('.', '\0').ToUpper());
            }
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (this.views[position % 3] == null)
            {
                this.views[position % 3] = LayoutInflater.From(container.Context)
                    .Inflate(Resource.Layout.page_schedule, container, false);
                container.AddView(this.views[position % 3]);
            }
            if (this.Schedule == null)
            {
                return this.views[position % 3];
            }
            var date = this.Schedule.IsByDate ?
                new DateTime(this.Schedule.GetSchedule(0).Day).AddDays(position) : this.FirstPosDate.AddDays(position);
            if (this.recyclerViews[position % 3] == null)
            {
                this.recyclerViews[position % 3] = this.views[position % 3]
                    .FindViewById<RecyclerView>(Resource.Id.recycler_schedule);
            }
            if (this.recyclerAdapters[position % 3] == null)
            {
                this.recyclerAdapters[position % 3] = new PairAdapter(
                       this.views[position % 3].FindViewById<TextView>(Resource.Id.text_null_lesson),
                       this.Schedule.GetSchedule(date, this.scheduleFilter),
                       this.scheduleFilter, date, this.showEmptyLessons, this.showColoredLessons, this.showGroup);
                this.recyclerViews[position % 3].SetItemAnimator(null);
                this.recyclerViews[position % 3].SetLayoutManager(new LinearLayoutManager(container.Context));
                this.recyclerViews[position % 3].SetAdapter(this.recyclerAdapters[position % 3]);
                this.recyclerAdapters[position % 3].LessonClick +=
                    lesson => LessonClick?.Invoke(lesson, this.recyclerAdapters[position % 3].Date);
            }
            else
            {
                this.recyclerAdapters[position % 3].BuildSchedule(this.Schedule.GetSchedule(date, this.scheduleFilter),
                    this.scheduleFilter, date, this.showEmptyLessons, this.showColoredLessons, this.showGroup);
            }
            if (this.textViews[position % 3] == null)
            {
                this.textViews[position % 3] = this.views[position % 3].FindViewById<TextView>(Resource.Id.text_day);
            }
            if (this.Schedule == null)
            {
                this.textViews[position % 3].Text = "Нет расписания";
            }
            if (this.Schedule.IsByDate)
            {
                this.textViews[position % 3].Text = new DateTime(this.Schedule.GetSchedule(0).Day)
                    .AddDays(position).ToString("dddd, d MMMM", this.customFormat);
            }
            else
            {
                this.textViews[position % 3].Text = this.FirstPosDate
                    .AddDays(position).ToString("dddd, d MMMM", this.customFormat);
            }
            return this.views[position % 3];
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }
        public override int GetItemPosition(Object @object)
        {
            return PagerAdapter.PositionNone;
        }

        public View GetView(int position)
        {
            return this.views[position % 3];
        }
    }
}