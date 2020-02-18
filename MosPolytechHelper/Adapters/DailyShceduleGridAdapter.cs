namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;
    using System.Globalization;
    using System.Linq;

    public class DailyShceduleGridAdapter : RecyclerView.Adapter
    {
        #region LessonTypeConstants
        const string CourseProject = "кп";
        const string Exam = "экзамен";
        const string Credit = "зачет";
        const string Consultation = "консультация";
        const string Laboratory = "лаб";
        const string Practice = "практика";
        const string Lecture = "лекция";
        const string Other = "другое";
        #endregion LessonTypeConstants

        readonly TextView nullMessage;
        Schedule schedule;
        Schedule.Filter scheduleFilter;
        CultureInfo customFormat;
        int itemCount;

        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(128, 74, 249),    // CourseProject
            new Color(235, 65, 65),     // Exam
            new Color(236, 105, 65),    // Credit
            new Color(227, 126, 200),   // Consultation
            new Color(236, 187, 93),    // Laboratory
            new Color(160, 212, 79),    // Practice
            new Color(116, 185, 244),   // Lecture
            new Color(193, 193, 193)    // Other
        };

        Color GetLessonTypeColor(string lessonType)
        {
            if (lessonType.Contains(CourseProject, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[0];
            }
            else if (lessonType.Contains(Exam, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[1];
            }
            else if (lessonType.Contains(Credit, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[2];
            }
            else if (lessonType.Contains(Consultation, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[3];
            }
            else if (lessonType.Contains(Laboratory, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[4];
            }
            else if (lessonType.Contains(Practice, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[5];
            }
            else if (lessonType.Contains(Lecture, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[6];
            }
            else if (lessonType.Contains(Other, StringComparison.OrdinalIgnoreCase))
            {
                return this.lessonTypeColors[7];
            }
            else
            {
                return Color.Gray;
            }
        }

        public override int ItemCount => this.itemCount;
        public event Action<DateTime> ItemClick;

        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }

        public DailyShceduleGridAdapter(TextView nullMessage, Schedule schedule, Schedule.Filter scheduleFilter,
            bool showEmptyLessons, bool showColoredLessons)
        {
            this.scheduleFilter = scheduleFilter;
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.schedule = schedule;
            SetCount(schedule);
            if (schedule != null)
            {
                SetFirstPosDate(schedule);
            }
            this.nullMessage = nullMessage;
            this.nullMessage.Visibility = this.schedule != null && this.schedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;

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

        public void BuildSchedule(Schedule schedule, Schedule.Filter scheduleFilter, bool showEmptyLessons, bool showColoredLessons)
        {
            this.scheduleFilter = scheduleFilter;
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.schedule = schedule;
            SetCount(schedule);
            if (schedule != null)
            {
                SetFirstPosDate(schedule);
            }
            this.nullMessage.Visibility = this.schedule != null && this.schedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int position)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_daily_schedule, parent, false);
            var vh = new ViewHolder(view);
            vh.LessonPlace.Click += (obj, arg) =>
            ItemClick?.Invoke(this.FirstPosDate.AddDays(vh.LayoutPosition));
            return vh;
        }

        void SetHead(ViewHolder viewHolder, DateTime date)
        {
            viewHolder.LessonTime.SetTextColor(new Color(120, 142, 161));
            viewHolder.LessonTime.SetText(date.ToString("ddd, d MMM", this.customFormat).Replace('.', '\0'), TextView.BufferType.Normal);
        }

        void SetLessons(ViewHolder viewHolder, Schedule.Daily dailySchedule, DateTime date)
        {
            string res = string.Empty;
            if (dailySchedule != null && dailySchedule.Count != 0)
            {
                string title;
                var time = dailySchedule[0].GetTime(date);
                res += time.StartTime + "-" + time.EndTime + "\t#" + (dailySchedule[0].Order + 1);
                title = dailySchedule[0].Title;
                //if (title.Length > 15)
                //{
                //    title = title.Substring(0, 15) + "...";
                //}
                res += "\n" + dailySchedule[0].Type.ToUpper() + "\n" + title;
                for (int i = 1; i < dailySchedule.Count; i++)
                {
                    if (!dailySchedule[i].EqualsTime(dailySchedule[i - 1], date))
                    {
                        time = dailySchedule[i].GetTime(date);
                        res += "\n" + time.StartTime + "-" + time.EndTime + "\t#" + (dailySchedule[i].Order + 1);
                    }
                    title = dailySchedule[i].Title;
                    //if (title.Length > 15)
                    //{
                    //    title = title.Substring(0, 15) + "...";
                    //}
                    res += "\n" + dailySchedule[i].Type.ToUpper() + "\n" + title;
                }
            }
            viewHolder.LessonType.SetText(res, TextView.BufferType.Normal);
        }

        void SetFirstPosDate(Schedule schedule)
        {
            if (!schedule.IsByDate)
            {
                this.FirstPosDate = this.itemCount == 400 ? DateTime.Today.AddDays(-200) : schedule.From;
            }
            else if (this.schedule == null)
            {
                this.FirstPosDate = DateTime.Today;
            }
            else
            {
                this.FirstPosDate = new DateTime(this.schedule.GetSchedule(0).Day);
            }
        }

        public DateTime FirstPosDate { get; private set; }

        public void SetCount(Schedule schedule)
        {
            if (schedule == null)
            {
                this.itemCount = 0;
            }
            else if (schedule.IsByDate)
            {
                this.itemCount = TimeSpan.FromTicks(Math.Abs(
                    schedule.GetSchedule(0).Day - schedule.GetSchedule(schedule.Count - 1).Day)).Days + 1;
            }
            else
            {
                this.itemCount = (schedule.To - schedule.From).Days + 1;
                if (this.itemCount > 400)
                {
                    this.itemCount = 400;
                }
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ViewHolder viewHolder) || this.schedule == null)
            {
                return;
            }
            DateTime date;
            if (this.schedule.IsByDate)
            {
                date = new DateTime(this.schedule.GetSchedule(0).Day).AddDays(position);
            }
            else
            {
                date = this.FirstPosDate.AddDays(position);
            }
            var dailySchedule = this.schedule.GetSchedule(date, this.scheduleFilter);
            SetLessons(viewHolder, dailySchedule, date);
            SetHead(viewHolder, date);
        }


        public class ViewHolder : RecyclerView.ViewHolder
        {
            public TextView LessonTime { get; }
            public TextView LessonType { get; }
            public LinearLayout LessonPlace { get; }

            public ViewHolder(View view) : base(view)
            {
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.text_schedule_time_grid);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.text_schedule_grid);
                this.LessonPlace = view.FindViewById<LinearLayout>(Resource.Id.linear_layout_schedule_grid);
            }
        }

        public class ItemDecoration : RecyclerView.ItemDecoration
        {
            readonly int offset;

            public ItemDecoration(int offset) : base()
            {
                this.offset = offset;
            }

            public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
            {
                outRect.Top = outRect.Bottom = outRect.Left = outRect.Right = this.offset;
            }
        }
    }
}