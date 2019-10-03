﻿namespace MosPolytechHelper.Adapters
{
    using Android.Support.V4.View;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using Java.Lang;
    using MosPolytechHelper.Domain;
    using System;
    using Object = Java.Lang.Object;

    public class ViewPagerAdapter : PagerAdapter
    {
        readonly View[] views;
        readonly RecyclerScheduleAdapter[] recyclerAdapter;
        Schedule schedule;
        int count;

        void SetFirstPos(bool isSession)
        {
            if (isSession)
            {
                this.FirstPos = 0;
            }
            else
            {
                this.FirstPos = DateTime.Today.DayOfYear;
            }
        }

        public int FirstPos { get; private set; }

        public ViewPagerAdapter(Schedule schedule)
        {
            this.schedule = schedule;
            SetCount(schedule);
            if (this.schedule != null)
            {
                SetFirstPos(schedule.IsSession);
            }
            this.recyclerAdapter = new RecyclerScheduleAdapter[3];
            this.views = new View[3];
        }

        public void UpdateSchedule(Schedule schedule)
        {
            var prev = this.schedule;
            this.schedule = schedule;
            SetCount(schedule);
            SetFirstPos(schedule.IsSession);

            if (prev == null && schedule != null)
            {
                NotifyDataSetChanged();
            }

        }

        public void UpdateSchedule2(Schedule schedule)
        {
            //this.recyclerAdapter[0]?.BuildSchedule(schedule?.GetSchedule(this.recyclerAdapter[0].Date),
            //    schedule.ScheduleFilter);
            //this.recyclerAdapter[1]?.BuildSchedule(schedule?.GetSchedule(this.recyclerAdapter[1].Date),
            //    schedule.ScheduleFilter);
            //this.recyclerAdapter[2]?.BuildSchedule(schedule?.GetSchedule(this.recyclerAdapter[2].Date),
            //    schedule.ScheduleFilter);
            NotifyDataSetChanged();
        }

        // TODO: Below
        public override int Count
        {
            get
            {
                return this.count;
            }
        }

        public void SetCount(Schedule schedule)
        {
            if (schedule == null)
            {
                this.count = 1;
            }
            else if (schedule.IsSession)
            {
                this.count = TimeSpan.FromTicks(System.Math.Abs(
                    schedule.GetSchedule(0).Day - schedule.GetSchedule(schedule.Count - 1).Day)).Days + 1;
            }
            else
            {
                this.count = 400;
            }
            NotifyDataSetChanged();
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            if (this.schedule == null)
            {
                return new Java.Lang.String("...");
            }
            if (this.schedule.IsSession)
            {
                return new Java.Lang.String(
                    DateTime.FromBinary(this.schedule.GetSchedule(0).Day).AddDays(position).ToString("ddd d MMM"));
            }
            else
            {
                return new Java.Lang.String(DateTime.Today.AddDays(position - this.FirstPos).ToString("ddd d MMM"));
            }
        }
        int counter;
        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (this.views[position % 3] == null)
            {
                var inflater = LayoutInflater.From(container.Context);
                var layout = (ViewGroup)inflater.Inflate(Resource.Layout.schedule, container, false);
                this.views[position % 3] = layout;
                container.AddView(this.views[position % 3]);
            }
            if (this.schedule == null)
                return this.views[position % 3];
            var date = this.schedule.IsSession ?
                        DateTime.FromBinary(this.schedule.GetSchedule(0).Day).AddDays(position) :
                        DateTime.Today.AddDays(position - this.FirstPos);
            if (this.recyclerAdapter[position % 3] == null)
            {
                
                var recyclerAdapter = new RecyclerScheduleAdapter(
                       this.views[position % 3].FindViewById<TextView>(Resource.Id.text_null_lesson), this.schedule?.GetSchedule(date),
                        this.schedule?.ScheduleFilter?.DateFitler == DateFilter.Desaturate, DateTime.MinValue, date); // TODO: Change it
                this.recyclerAdapter[position % 3] = recyclerAdapter;
                var recyclerView = this.views[position % 3].FindViewById<RecyclerView>(Resource.Id.recycler_student_schedule);
                recyclerView.SetLayoutManager(new LinearLayoutManager(container.Context));
                recyclerView.SetAdapter(this.recyclerAdapter[position % 3]);
            }
            else
            {
                this.recyclerAdapter[position % 3].BuildSchedule(schedule?.GetSchedule(date),
                schedule.ScheduleFilter, date);
            }
            return this.views[position % 3];
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            //container.RemoveView(@object as View);
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }
        public override void SetPrimaryItem(ViewGroup container, int position, Object @object)
        {
            base.SetPrimaryItem(container, position, @object);
        }
        public override int GetItemPosition(Object @object)
        {
            return PagerAdapter.PositionNone;
        }
    }
}