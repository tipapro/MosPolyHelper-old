namespace MosPolytechHelper.Adapters
{
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Domain;
    using System;

    public class RecyclerTimetableAdapter : RecyclerView.Adapter
    {
        TextView nullMessage;
        DailyTimetable timetable;
        bool isSession;
        DateTime groupDateFrom;
        DateTime date;
        View prevLineView;
        int prevPosition;

        public void BuildTimetable(FullTimetable fullTimetable)
        {
            var oldValue = this.timetable;
            this.timetable = fullTimetable?.GetTimetable(this.date);
            if (oldValue == null && this.timetable != null && this.ItemCount != 0)
            {
                ChangeNullMessageVisibility(ViewStates.Invisible);
            }
            else if (oldValue != null && this.timetable == null || this.ItemCount == 0)
            {
                ChangeNullMessageVisibility(ViewStates.Visible);
            }
            NotifyDataSetChanged();
        }

        public void ChangeNullMessageVisibility(ViewStates viewState)
        {
            this.nullMessage.Visibility = viewState;
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class TimetableViewHolder : RecyclerView.ViewHolder
        {
            public TextView LessonTime { get; }
            public TextView LessonTitle { get; }
            public TextView LessonType { get; }
            public TextView LessonTeachers { get; }
            public LinearLayout LessonLayout { get; }
            public RecyclerView RecyclerAuditorium { get; }
            public TextView LessonModuleAndWeekType { get; }
            public TextView LessonDate { get; }
            public View LessonLine { get; }

            public TimetableViewHolder(View view) : base(view)
            {
                //this.LessonLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_student_timetable);
                this.LessonTitle = view.FindViewById<TextView>(Resource.Id.text_student_timetable_title);
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.student_timetable_time);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.student_timetable_type);
                this.LessonTeachers = view.FindViewById<TextView>(Resource.Id.text_student_timetable_teachers);
                this.RecyclerAuditorium = view.FindViewById<RecyclerView>(Resource.Id.recycler_auditoriums);
                this.LessonModuleAndWeekType = view.FindViewById<TextView>(Resource.Id.text_student_timetable_module_and_week_type);
                this.LessonDate = view.FindViewById<TextView>(Resource.Id.text_student_timetable_date);
                this.LessonLayout = view.FindViewById<LinearLayout>(Resource.Id.linear_layout_student_timetable);
                this.LessonLine = view.FindViewById<View>(Resource.Id.line_student_timetable);
            }
        }

        // Initialize the dataset of the Adapter
        public RecyclerTimetableAdapter(TextView nullMessage, DailyTimetable timetable, bool isSession, DateTime groupDateFrom, DateTime date)
        {
            this.prevPosition = -1;
            this.nullMessage = nullMessage;
            this.timetable = timetable;
            if (this.timetable == null || timetable.Count == 0)
                ChangeNullMessageVisibility(ViewStates.Visible);
            this.isSession = isSession;
            this.groupDateFrom = groupDateFrom;
            this.date = date;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.item_student_timetable, viewGroup, false);
            view.Enabled = false;
            return new TimetableViewHolder(view);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            // Get element from your dataset at this position and replace the contents of the view
            // with that element
            var temp = this.timetable?[position];
            if (temp == null)
            {
                return;
            }
            string title = "null";
            string timeStart = "null";
            string timeEnd = "null";
            string type = "null";
            string teachers = "";
            string moduleAndWeelType = string.Empty;
            if (this.prevPosition != -1 && temp.Order != this.prevPosition)
            {
                this.prevLineView.Visibility = ViewStates.Visible;
            }
            if (position == ItemCount - 1)
            {
                this.prevPosition = -1;
                (viewHolder as TimetableViewHolder).LessonLine.Visibility = ViewStates.Visible;
            }
            else
            {
                this.prevPosition = temp.Order;
                this.prevLineView = (viewHolder as TimetableViewHolder).LessonLine;
            }
            if (position == 0)
            {
                float scale = (viewHolder as TimetableViewHolder).LessonLayout.
                    Context.Resources.DisplayMetrics.Density;
                int padding5InPx = (int)(5 * scale + 0.5f);
                int padding2InPx = (int)(2 * scale + 0.5f);
                (viewHolder as TimetableViewHolder).LessonLayout.SetPadding(padding5InPx, padding5InPx, padding5InPx, padding2InPx);
            }
            if (temp.Week == WeekType.None)
            {
                //((viewHolder as TimetableViewHolder).LessonLayout.Background as GradientDrawable).SetColor(
                //new Android.Graphics.Color(255, 255, 255));
            }
            else if (temp.Week == WeekType.Odd)
            {
                moduleAndWeelType = "Odd week";
                //((viewHolder as TimetableViewHolder).LessonLayout.Background as GradientDrawable).SetColor(
                //new Android.Graphics.Color(204, 204, 204));
            }
            else if (temp.Week == WeekType.Even)
            {
                moduleAndWeelType = "Even week";
                //((viewHolder as TimetableViewHolder).LessonLayout.Background as GradientDrawable).SetColor(
                //new Android.Graphics.Color(255, 255, 230));
            }
            if (temp.Module == Module.First)
            {
                moduleAndWeelType = moduleAndWeelType == string.Empty ? "First module" : ", First module";
            }
            else if (temp.Module == Module.Second)
            {
                moduleAndWeelType = moduleAndWeelType == string.Empty ? "Second module" : ", Second module";
            }
            string date;
            if (temp.DateFrom == temp.DateTo)
            {
                date = temp.DateFrom.ToString("d MMM");
            }
            else
            {
                date = temp.DateFrom.ToString("d MMM") + " - " + temp.DateTo.ToString("d MMM");
            }
            title = temp.SubjectName;
            (timeStart, timeEnd) = DailyTimetable.GetLessonTime(temp.Order, this.isSession, this.groupDateFrom);
            type = temp.Type;
            teachers = string.Join(", ", temp.GetShortTeacherNames());
            var adapter = new AuditoriumsAdapter(temp.Auditoriums);
            (viewHolder as TimetableViewHolder).RecyclerAuditorium.SetLayoutManager(
                new LinearLayoutManager((viewHolder as TimetableViewHolder).LessonTime.Context, LinearLayoutManager.Horizontal, false));
            (viewHolder as TimetableViewHolder).RecyclerAuditorium.SetAdapter(adapter);
            (viewHolder as TimetableViewHolder).LessonTitle.SetText(title, TextView.BufferType.Normal);
            (viewHolder as TimetableViewHolder).LessonTime.SetText(timeStart + " - " + timeEnd, TextView.BufferType.Normal);
            (viewHolder as TimetableViewHolder).LessonType.SetText(type, TextView.BufferType.Normal);
            (viewHolder as TimetableViewHolder).LessonTeachers.SetText(teachers, TextView.BufferType.Normal);
            (viewHolder as TimetableViewHolder).LessonModuleAndWeekType.SetText(moduleAndWeelType, TextView.BufferType.Normal);
            (viewHolder as TimetableViewHolder).LessonDate.SetText(date, TextView.BufferType.Normal);
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get => this.timetable?.Count ?? 0;
        }
    }
}