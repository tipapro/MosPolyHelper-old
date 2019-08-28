namespace MosPolytechHelper.Adapters
{
    using Android.Content;
    using Android.Support.V4.View;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using Java.Lang;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using System;
    using Object = Java.Lang.Object;

    public class ViewPagerAdapter : PagerAdapter
    {
        event Action<FullTimetable> OnTimetableChanged;

        ILoggerFactory loggerFactory;
        Context context;

        FullTimetable fullTimetable;
        public int FirstPos { get; }
        //readonly Fragment[] fragments;

        //public ViewPagerAdapter(FragmentManager fm, Fragment[] fragments) : base(fm)
        //{
        //    this.fragments = fragments;
        //}

        //public override int Count => this.fragments.Length;

        //public override Fragment GetItem(int position) => this.fragments[position];

        public ViewPagerAdapter(Context context, FullTimetable fullTimetable)
        {
            this.context = context;
            this.FirstPos = this.Count / 2;
            this.fullTimetable = fullTimetable;
        }

        public void UpdateTimetable(FullTimetable fullTimetable)
        {
            this.fullTimetable = fullTimetable;
            OnTimetableChanged.Invoke(fullTimetable);
        }

        // TODO: Below
        public override int Count => int.MaxValue;

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(DateTime.Today.AddDays(position - this.FirstPos).ToString("ddd d MMM"));
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            var inflater = LayoutInflater.From(this.context);
            var layout = (ViewGroup)inflater.Inflate(Resource.Layout.fragment_timetable, container, false);


            var recyclerView = layout.FindViewById<RecyclerView>(Resource.Id.recycler_student_timetable);

            // A LinearLayoutManager is used here, this will layout the elements in a similar fashion
            // to the way ListView would layout elements. The RecyclerView.LayoutManager defines how the
            // elements are laid out.
            recyclerView.SetLayoutManager(new LinearLayoutManager(this.context));
            var date = DateTime.Today.AddDays(position - this.FirstPos);
            var adapter = new RecyclerTimetableAdapter(
                layout.FindViewById<TextView>(Resource.Id.text_null_lesson), this.fullTimetable?.GetTimetable(date),
                false, DateTime.MinValue, date); // TODO: Change it
            OnTimetableChanged += adapter.BuildTimetable;
            // Set CustomAdapter as the adapter for RecycleView
            recyclerView.SetAdapter(adapter);

            container.AddView(layout);
            return layout;
        }
        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            container.RemoveView(@object as View);
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }
    }
}