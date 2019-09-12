namespace MosPolytechHelper.Adapters
{
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Domain;
    using System;

    public class RecyclerScheduleAdapter : RecyclerView.Adapter
    {
        #region LessonTypeConstants
        const string Credit = "зачет";
        const string Exam = "экзамен";
        const string Practice = "практика";
        const string Lecture = "лекция";
        const string Laboratory = "лаб";
        const string Work = "работа";
        #endregion LessonTypeConstants

        TextView nullMessage;
        Schedule.Daily schedule;
        bool desaturateOtherDates;
        bool isSession;
        DateTime groupDateFrom;
        DateTime date;
        (Color, Color)[] colors = new (Color, Color)[]
        {
            (new Color(233, 89, 73), new Color(248, 214, 211)),   // Red
            (new Color(15, 189, 88), new Color(215, 244, 228)),  // Green
            (new Color(11, 115, 218), new Color(218, 230, 241)), // Blue
            (new Color(246, 206, 85), new Color(246, 237, 213)),  // Yellow
            (new Color(199, 90, 242), new Color(246, 233, 252)), // Purple
            (new Color(160, 170, 171), new Color(241, 243, 243)),    // DarkGrey
        };
        Color[] lessonTypeColors = new Color[]
        {
            new Color(246, 96, 171)/* Credit - Pink*/, new Color(248, 79, 22)/* Exam - Red */,
            new Color(59, 185, 255)/* Practice - Blue */, new Color(255, 182, 133)/* Lecture - Yellow */,
            new Color(76, 196, 23)/* Laboratory Work - Green */
        };

        Color GetLessonTypeColor(string lessonType)
        {
            if (lessonType.Contains(Credit, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[0];
            }
            else if (lessonType.Contains(Exam, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[1];
            }
            else if (lessonType.Contains(Practice, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[2];
            }
            else if (lessonType.Contains(Lecture, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[3];
            }
            else if (lessonType.Contains(Laboratory, StringComparison.OrdinalIgnoreCase) &&
                lessonType.Contains(Work, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[4];
            }
            else
            {
                return Color.Gray;
            }
        }

        void SetUpColors(bool enabled, ScheduleViewHolder viewHolder, AuditoriumsAdapter adapter, Lesson lesson)
        {
            if (lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
            {
                (viewHolder.LessonLayout.Background as GradientDrawable).SetColor(
                new Color(255, 230, 234));  // Pink
                enabled = true;
            }
            else
            {
                (viewHolder.LessonLayout.Background as GradientDrawable).SetColor(
                new Color(255, 255, 255));  // White
            }

            float scale = viewHolder.LessonLayout.
                   Context.Resources.DisplayMetrics.Density;
            int padding5InPx = (int)(5 * scale + 0.5f);
            int padding2InPx = (int)(1 * scale + 0.5f);

            viewHolder.LessonTime.Enabled = enabled;
            viewHolder.LessonTitle.Enabled = enabled;
            viewHolder.RecyclerAuditorium.Enabled = enabled;
            viewHolder.LessonType.Enabled = enabled;
            viewHolder.LessonTeachers.Enabled = enabled;
            viewHolder.LessonModuleAndWeekType.Enabled = enabled;
            viewHolder.LessonDate.Enabled = enabled;
            viewHolder.LessonLayout.Enabled = enabled;
            adapter.Enabled = enabled;


            var lessonTimeBack = (viewHolder.LessonTime.Background as GradientDrawable);
            var lessonLayoutBack = (viewHolder.LessonLayout.Background as GradientDrawable);
            if (enabled)
            {
                viewHolder.LessonTime.SetTextColor(Color.White);
                viewHolder.LessonType.SetTextColor(GetLessonTypeColor(lesson.Type));
                viewHolder.LessonModuleAndWeekType.SetTextColor(new Color(170, 170, 170));
                viewHolder.LessonDate.SetTextColor(new Color(170, 170, 170));
                viewHolder.LessonTitle.SetTextColor(new Color(110, 110, 110));
                lessonTimeBack.SetColor(this.colors[lesson.Order % this.colors.Length].Item1);

                lessonLayoutBack.SetStroke(padding2InPx, Color.Gray);
                lessonTimeBack.SetStroke(padding2InPx, Color.Gray);
            }
            else
            {
                viewHolder.LessonTime.SetTextColor(new Color(250, 250, 250));
                viewHolder.LessonType.SetTextColor(new Color(225, 225, 225));
                viewHolder.LessonModuleAndWeekType.SetTextColor(new Color(230, 230, 230));
                viewHolder.LessonDate.SetTextColor(new Color(230, 230, 230));
                viewHolder.LessonTitle.SetTextColor(new Color(215, 215, 215));
                lessonTimeBack.SetColor(this.colors[lesson.Order % this.colors.Length].Item2);

                lessonLayoutBack.SetStroke(padding2InPx, Color.LightGray);
                lessonTimeBack.SetStroke(padding2InPx, Color.LightGray);
            }

        }

        public void BuildSchedule(Schedule fullSchedule)
        {
            this.schedule = fullSchedule?.GetSchedule(this.date);
            this.desaturateOtherDates = fullSchedule?.ScheduleFilter?.DateFitler == DateFilter.Desaturate;
            if (this.schedule != null && this.schedule.Count != 0)
            {
                this.nullMessage.Visibility = ViewStates.Invisible;
            }
            else
            {
                this.nullMessage.Visibility = ViewStates.Visible;
            }
            NotifyDataSetChanged();
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class ScheduleViewHolder : RecyclerView.ViewHolder
        {
            public TextView LessonTime { get; }
            public TextView LessonTitle { get; }
            public TextView LessonType { get; }
            public TextView LessonTeachers { get; }
            public RelativeLayout LessonLayout { get; }
            public RecyclerView RecyclerAuditorium { get; }
            public TextView LessonModuleAndWeekType { get; }
            public TextView LessonDate { get; }
            //public View LessonLine { get; }

            public ScheduleViewHolder(View view) : base(view)
            {
                //this.LessonLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_student_schedule);
                this.LessonTitle = view.FindViewById<TextView>(Resource.Id.text_student_schedule_title);
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.student_schedule_time);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.student_schedule_type);
                this.LessonTeachers = view.FindViewById<TextView>(Resource.Id.text_student_schedule_teachers);
                this.RecyclerAuditorium = view.FindViewById<RecyclerView>(Resource.Id.recycler_auditoriums);
                this.LessonModuleAndWeekType = view.FindViewById<TextView>(Resource.Id.text_student_schedule_module_and_week_type);
                this.LessonDate = view.FindViewById<TextView>(Resource.Id.text_student_schedule_date);
                this.LessonLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_student_schedule);
                //this.LessonLine = view.FindViewById<View>(Resource.Id.line_student_schedule);
            }
        }

        // Initialize the dataset of the Adapter
        public RecyclerScheduleAdapter(TextView nullMessage, Schedule.Daily schedule,
            bool isSession, bool desaturateOtherDates, DateTime groupDateFrom, DateTime date)
        {
            this.nullMessage = nullMessage;
            this.schedule = schedule;
            if (this.schedule == null || schedule.Count == 0)
                this.nullMessage.Visibility = ViewStates.Visible;
            this.isSession = isSession;
            this.desaturateOtherDates = desaturateOtherDates;
            this.groupDateFrom = groupDateFrom;
            this.date = date;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.item_student_schedule, viewGroup, false);
            view.Enabled = false;
            return new ScheduleViewHolder(view);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            var viewHolder = vh as ScheduleViewHolder;
            // Get element from your dataset at this position and replace the contents of the view
            // with that element
            var temp = this.schedule?[position];
            if (temp == null)
            {
                return;
            }

            string moduleAndWeelType = string.Empty;

            float scale = viewHolder.LessonLayout.
                   Context.Resources.DisplayMetrics.Density;
            int padding5InPx = (int)(5 * scale + 0.5f);
            int padding2InPx = (int)(2 * scale + 0.5f);

            var adapter = new AuditoriumsAdapter(temp.Auditoriums);

            if (position == 0)
            {
                var layoutParams = viewHolder.LessonLayout.LayoutParameters as LinearLayout.LayoutParams;
                layoutParams.SetMargins(0, padding5InPx, 0, padding5InPx);
                viewHolder.LessonLayout.LayoutParameters = layoutParams;
            }

            SetUpColors(temp.DateFrom <= this.date && temp.DateTo >= this.date || !this.desaturateOtherDates,
                viewHolder, adapter, temp);


            if (temp.Week == WeekType.Odd)
            {
                moduleAndWeelType = viewHolder.LessonLayout.
                   Context.Resources.GetString(Resource.String.odd_week);
            }
            else if (temp.Week == WeekType.Even)
            {
                moduleAndWeelType = viewHolder.LessonLayout.
                   Context.Resources.GetString(Resource.String.even_week);
            }

            if (temp.Module == Module.First)
            {
                string firstModule = viewHolder.LessonLayout.
                   Context.Resources.GetString(Resource.String.first_module);

                if (moduleAndWeelType == string.Empty)
                    moduleAndWeelType = firstModule;
                else
                    moduleAndWeelType = ", " + firstModule;
            }
            else if (temp.Module == Module.Second)
            {
                string secondModule = viewHolder.LessonLayout.
                   Context.Resources.GetString(Resource.String.second_module);

                if (moduleAndWeelType == string.Empty)
                    moduleAndWeelType = secondModule;
                else
                    moduleAndWeelType = ", " + secondModule;
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
            string title = temp.SubjectName;

            string timeStart;

            string timeEnd;
            (timeStart, timeEnd) = Schedule.Daily.GetLessonTime(temp.Order, this.isSession, this.groupDateFrom);
            string type = temp.Type;
            string teachers = string.Join(", ", temp.GetShortTeacherNames());

            viewHolder.RecyclerAuditorium.SetLayoutManager(
                new LinearLayoutManager(viewHolder.LessonTime.Context, LinearLayoutManager.Horizontal, false));
            viewHolder.RecyclerAuditorium.SetAdapter(adapter);
            viewHolder.LessonTitle.SetText(title, TextView.BufferType.Normal);
            string time = timeStart + " - " + timeEnd;
            viewHolder.LessonTime.SetText(time, TextView.BufferType.Normal);
            viewHolder.LessonType.SetText(type, TextView.BufferType.Normal);
            viewHolder.LessonTeachers.SetText(teachers, TextView.BufferType.Normal);
            viewHolder.LessonModuleAndWeekType.SetText(moduleAndWeelType, TextView.BufferType.Normal);
            viewHolder.LessonDate.SetText(date, TextView.BufferType.Normal);
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get => this.schedule?.Count ?? 0;
        }
    }
}