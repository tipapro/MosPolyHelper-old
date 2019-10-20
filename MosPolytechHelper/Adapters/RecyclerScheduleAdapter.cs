namespace MosPolytechHelper.Adapters
{
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Support.V7.Widget;
    using Android.Text;
    using Android.Text.Style;
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
        Schedule.Daily dailySchedule;
        bool desaturateOtherDates;
        DateTime groupDateFrom;
        readonly (Color, Color)[] lessonTimeColors = new (Color, Color)[]
        {
            (new Color(233, 89, 73), new Color(248, 214, 211)),   // Red
            (new Color(255, 171, 0), new Color(255, 238, 204)),  // Orange
            (new Color(15, 189, 88), new Color(195, 239, 215)),  // Green
            (new Color(68, 194, 255), new Color(204, 241, 255)), // Blue
            (new Color(62, 104, 211), new Color(193, 206, 241)), // DarkBlue
            (new Color(100, 31, 173), new Color(235, 201, 255)), // Purple
            (new Color(84, 94, 95), new Color(227, 232, 232))    // DarkGrey
        };
        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(246, 96, 171)/* Credit - Pink*/,
            new Color(248, 79, 22)/* Exam - Red */,
            new Color(59, 185, 255)/* Practice - Blue */,
            new Color(255, 182, 133)/* Lecture - Yellow */,
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

        void SetUpColors(bool enabled, ScheduleViewHolder viewHolder, Lesson lesson, SpannableStringBuilder auditoriums)
        {
            if (lesson.Type.Contains("зачет", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("экзамен", StringComparison.OrdinalIgnoreCase) ||
                lesson.Type.Contains("зачёт", StringComparison.OrdinalIgnoreCase))
            {
                (viewHolder.LessonLayout.Background as GradientDrawable).SetColor(new Color(255, 218, 213));  // Pink
                enabled = true;
            }
            else
            {
                (viewHolder.LessonLayout.Background as GradientDrawable).SetColor(new Color(255, 255, 255));  // White
            }

            float scale = viewHolder.LessonLayout.
                   Context.Resources.DisplayMetrics.Density;
            int padding1InPx = (int)(1 * scale + 0.5f);

            viewHolder.LessonTime.Enabled = enabled;
            viewHolder.LessonTitle.Enabled = enabled;
            viewHolder.LessonAuditoriums.Enabled = enabled;
            viewHolder.LessonType.Enabled = enabled;
            viewHolder.LessonTeachers.Enabled = enabled;
            viewHolder.LessonModuleAndWeekType.Enabled = enabled;
            viewHolder.LessonDate.Enabled = enabled;
            viewHolder.LessonLayout.Enabled = enabled;

            var lessonTimeBackground = (viewHolder.LessonTime.Background as GradientDrawable);
            var lessonLayoutBackground = (viewHolder.LessonLayout.Background as GradientDrawable);
            if (enabled)
            {
                viewHolder.LessonTime.SetTextColor(Color.White);
                viewHolder.LessonModuleAndWeekType.SetTextColor(new Color(170, 170, 170));
                viewHolder.LessonType.SetTextColor(GetLessonTypeColor(lesson.Type));
                viewHolder.LessonDate.SetTextColor(new Color(170, 170, 170));
                viewHolder.LessonTitle.SetTextColor(new Color(110, 110, 110));
                lessonTimeBackground.SetColor(this.lessonTimeColors[lesson.Order % this.lessonTimeColors.Length].Item1);

                for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                {
                    auditoriums.Append(lesson.Auditoriums[i].Name.Replace(" ", "\u00A0") + ", ",
                        new ForegroundColorSpan(Color.ParseColor(lesson.Auditoriums[i].Color)), SpanTypes.ExclusiveExclusive);
                }
                if (lesson.Auditoriums.Length != 0)
                {
                    auditoriums.Append(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Name.Replace(" ", "\u00A0"),
                        new ForegroundColorSpan(Color.ParseColor(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Color)),
                        SpanTypes.ExclusiveExclusive);
                }
            }
            else
            {
                viewHolder.LessonTime.SetTextColor(new Color(250, 250, 250));
                viewHolder.LessonModuleAndWeekType.SetTextColor(new Color(230, 230, 230));
                viewHolder.LessonType.SetTextColor(new Color(225, 225, 225));
                viewHolder.LessonDate.SetTextColor(new Color(230, 230, 230));
                viewHolder.LessonTitle.SetTextColor(new Color(215, 215, 215));
                lessonTimeBackground.SetColor(this.lessonTimeColors[lesson.Order % this.lessonTimeColors.Length].Item2);

                lessonLayoutBackground.SetColor(new Color(248, 248, 248));

                for (int i = 0; i < lesson.Auditoriums.Length - 1; i++)
                {
                    auditoriums.Append(lesson.Auditoriums[i].Name + ", ",
                        new ForegroundColorSpan(new Color(225, 225, 225)), SpanTypes.ExclusiveExclusive);
                }
                if (lesson.Auditoriums.Length != 0)
                {
                    auditoriums.Append(lesson.Auditoriums[lesson.Auditoriums.Length - 1].Name,
                        new ForegroundColorSpan(new Color(225, 225, 225)),
                        SpanTypes.ExclusiveExclusive);
                }
            }
        }

        public override int ItemCount =>
            this.dailySchedule?.Count ?? 0;

        public DateTime Date { get; set; }
        public bool GroupIsEvening { get; set; }
        public event Action DailyScheduleChanged;

        public RecyclerScheduleAdapter(TextView nullMessage, Schedule.Daily dailySchedule,
            bool desaturateOtherDates, DateTime groupDateFrom, DateTime date, bool groupIsEvening)
        {
            this.nullMessage = nullMessage;
            this.dailySchedule = dailySchedule;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Invisible : ViewStates.Visible;
            this.desaturateOtherDates = desaturateOtherDates;
            this.groupDateFrom = groupDateFrom;
            this.Date = date;
            this.GroupIsEvening = groupIsEvening;
        }

        public void BuildSchedule(Schedule.Daily dailySchedule, Schedule.Filter scheduleFilter, DateTime date)
        {
            this.dailySchedule = dailySchedule;
            this.Date = date;
            this.desaturateOtherDates = scheduleFilter?.DateFitler == DateFilter.Desaturate;
            this.nullMessage.Visibility = this.dailySchedule != null && this.dailySchedule.Count != 0 ?
                ViewStates.Invisible : ViewStates.Visible;
            DailyScheduleChanged?.Invoke();
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context).Inflate(Resource.Layout.item_schedule, viewGroup, false);
            view.Enabled = false;
            var vh = new ScheduleViewHolder(view, OnItemClick);
            DailyScheduleChanged += vh.OnDailyScheduleChanged;
            return vh;
        }

        void OnItemClick(int position)
        {
            NotifyItemChanged(position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(this.dailySchedule?.GetLesson(position) is Lesson lesson))
            {
                return;
            }
            var viewHolder = vh as ScheduleViewHolder;
            if (position == 0)
            {
                float scale = viewHolder.LessonLayout.Context.Resources.DisplayMetrics.Density;
                int padding4InPx = (int)(4 * scale + 0.5f);
                var layoutParams = viewHolder.LessonLayout.LayoutParameters as LinearLayout.LayoutParams;
                layoutParams.SetMargins(0, padding4InPx, 0, padding4InPx);
                viewHolder.LessonLayout.LayoutParameters = layoutParams;
            }
            var auditoriums = new SpannableStringBuilder();
            SetUpColors(lesson.DateFrom <= this.Date && lesson.DateTo >= this.Date || !this.desaturateOtherDates,
                viewHolder, lesson, auditoriums);
            // Display dates
            if (lesson.DateFrom != DateTime.MinValue && lesson.DateTo != DateTime.MaxValue)
            {
                string date;
                if (lesson.DateFrom == lesson.DateTo)
                {
                    date = lesson.DateFrom.ToString("d MMM");
                }
                else
                {
                    date = lesson.DateFrom.ToString("d MMM") + " - " + lesson.DateTo.ToString("d MMM");
                }
                viewHolder.LessonDate.SetText(date, TextView.BufferType.Normal);
                viewHolder.LessonDate.Visibility = ViewStates.Visible;
            }
            else
            {
                viewHolder.LessonDate.Visibility = ViewStates.Gone;
            }
            // Display time
            var (timeStart, timeEnd) = Lesson.GetLessonTime(this.Date, lesson.Order, this.GroupIsEvening, this.groupDateFrom);
            string time = timeStart + " - " + timeEnd;
            viewHolder.LessonTime.SetText(time, TextView.BufferType.Normal);
            // Display lesson title
            var title = lesson.Title;
            viewHolder.LessonTitle.SetText(title, TextView.BufferType.Normal);
            // Display lesson type
            var type = lesson.Type;
            viewHolder.LessonType.SetText(type, TextView.BufferType.Normal);
            // Display teachers
            string teachers = string.Join(", ", viewHolder.ShowFullTeacherName ?
                lesson.GetFullTecherNames() : lesson.GetShortTeacherNames());
            if (string.IsNullOrEmpty(teachers))
            {
                viewHolder.LessonTeachers.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.LessonTeachers.SetText(teachers, TextView.BufferType.Normal);
                viewHolder.LessonTeachers.Visibility = ViewStates.Visible;
            }
            // Display auditoriums
            viewHolder.LessonAuditoriums.SetText(auditoriums, TextView.BufferType.Normal);
            //viewHolder.LessonAuditoriums.Ellipsize = TextUtils.TruncateAt.Marquee;
            // Display type of week
            string moduleAndWeelType = string.Empty;
            if (lesson.Week != WeekType.None)
            {
                if (lesson.Week == WeekType.Odd)
                {
                    moduleAndWeelType = viewHolder.LessonLayout.
                       Context.Resources.GetString(Resource.String.odd_week);
                }
                else
                {
                    moduleAndWeelType = viewHolder.LessonLayout.
                       Context.Resources.GetString(Resource.String.even_week);
                }
            }
            // Display module
            if (lesson.Module != Module.None)
            {
                string module = viewHolder.LessonLayout.Context.Resources.GetString(lesson.Module == Module.First ?
                    Resource.String.first_module : Resource.String.second_module);
                if (moduleAndWeelType == string.Empty)
                {
                    moduleAndWeelType = module;
                }
                else
                {
                    moduleAndWeelType = ", " + module;
                }
            }
            if (string.IsNullOrEmpty(moduleAndWeelType))
            {
                viewHolder.LessonModuleAndWeekType.Visibility = ViewStates.Gone;
            }
            else
            {
                viewHolder.LessonModuleAndWeekType.SetText(moduleAndWeelType, TextView.BufferType.Normal);
                viewHolder.LessonModuleAndWeekType.Visibility = ViewStates.Visible;
            }
        }


        public class ScheduleViewHolder : RecyclerView.ViewHolder
        {
            public bool ShowFullTeacherName { get; set; }
            public TextView LessonTime { get; }
            public TextView LessonTitle { get; }
            public TextView LessonType { get; }
            public TextView LessonAuditoriums { get; }
            public TextView LessonTeachers { get; }
            public RelativeLayout LessonLayout { get; }
            public TextView LessonModuleAndWeekType { get; }
            public TextView LessonDate { get; }
            public ScheduleViewHolder(View view, Action<int> OnItemClick) : base(view)
            {
                this.ShowFullTeacherName = false;
                this.LessonTitle = view.FindViewById<TextView>(Resource.Id.text_schedule_title);
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.text_schedule_time);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.text_schedule_type);
                this.LessonTeachers = view.FindViewById<TextView>(Resource.Id.text_schedule_teachers);
                this.LessonAuditoriums = view.FindViewById<TextView>(Resource.Id.text_schedule_auditoriums);
                this.LessonAuditoriums.ViewTreeObserver.GlobalLayout += (obj, arg) =>
                {
                    this.LessonAuditoriums.Selected = true;
                    var a = this.LessonAuditoriums.Ellipsize;
                };
                this.LessonModuleAndWeekType = view.FindViewById<TextView>(Resource.Id.text_schedule_module_and_week_type);
                this.LessonDate = view.FindViewById<TextView>(Resource.Id.text_schedule_date);
                this.LessonLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule);
                this.LessonLayout.Click += (obj, arg) =>
                {
                    this.ShowFullTeacherName = !this.ShowFullTeacherName;
                    OnItemClick(this.LayoutPosition);
                };
            }

            public void OnDailyScheduleChanged()
            {
                this.ShowFullTeacherName = false;
            }
        }
    }
}