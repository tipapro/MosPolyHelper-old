namespace MosPolyHelper.Adapters
{
    using Android.Support.V4.View;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using Java.Lang;
    using MosPolyHelper.Domain;
    using System;
    using Object = Java.Lang.Object;

    public class DailyShedulePageAdapter : PagerAdapter
    {
        readonly View[] views;
        readonly PairAdapter[] recyclerAdapters;
        readonly RecyclerView[] recyclerViews;
        public Schedule Schedule;
        Schedule.Filter scheduleFilter;
        bool showEmptyLessons;
        bool showColoredLessons;
        int count;

        void SetFirstPosDate(bool isSession)
        {
            if (!isSession)
            {
                this.FirstPosDate = DateTime.Today.AddDays(-this.Count / 2);
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
        public override int Count => this.count;

        public DailyShedulePageAdapter(Schedule schedule, Schedule.Filter scheduleFilter,
            bool showEmptyLessons, bool showColoredLessons)
        {
            this.Schedule = schedule;
            this.scheduleFilter = scheduleFilter;
            this.showEmptyLessons = showEmptyLessons;
            this.showColoredLessons = showColoredLessons;
            SetCount(schedule);
            if (this.Schedule != null)
            {
                SetFirstPosDate(schedule.IsByDate);
            }
            this.recyclerAdapters = new PairAdapter[3];
            this.views = new View[3];
            this.recyclerViews = new RecyclerView[3];
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
                this.count = 200 * 2;
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
                    .AddDays(position).ToString(" ddd d MMM ").Replace('.', '\0'));
            }
            else
            {
                return new Java.Lang.String(this.FirstPosDate
                    .AddDays(position).ToString(" ddd d MMM ").Replace('.', '\0'));
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
                       this.Schedule.GetSchedule(date, scheduleFilter),
                       scheduleFilter, date, this.Schedule.Group,
                       this.showEmptyLessons, this.showColoredLessons);
                this.recyclerViews[position % 3].SetItemAnimator(null);
                this.recyclerViews[position % 3].SetLayoutManager(new LinearLayoutManager(container.Context));
                this.recyclerViews[position % 3].SetAdapter(this.recyclerAdapters[position % 3]);
            }
            else
            {
                this.recyclerAdapters[position % 3].BuildSchedule(this.Schedule.GetSchedule(date, scheduleFilter),
                    scheduleFilter, date, this.Schedule.Group, this.showEmptyLessons, this.showColoredLessons);
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
    }
}