namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolyHelper.Domain;
    using System;

    public class RecyclerScheduleGridAdapter : RecyclerView.Adapter
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

        public bool ShowEmptyLessons { get; set; }
        public bool ShowColoredLessons { get; set; }
        public event Action DailyScheduleChanged;

        public RecyclerScheduleGridAdapter(TextView nullMessage, Schedule schedule, bool showEmptyLessons, bool showColoredLessons)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.schedule = schedule;
            SetCount(schedule);
            bool? isSession = schedule?.IsByDate;
            SetFirstPosDate(isSession ?? false);
            this.nullMessage = nullMessage;
            this.nullMessage.Visibility = this.schedule != null && this.schedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
        }

        public void BuildSchedule(Schedule schedule, bool showEmptyLessons, bool showColoredLessons)
        {
            this.ShowEmptyLessons = showEmptyLessons;
            this.ShowColoredLessons = showColoredLessons;
            this.schedule = schedule;
            SetCount(schedule);
            bool? isSession = schedule?.IsByDate;
            SetFirstPosDate(isSession ?? false);
            this.nullMessage.Visibility = this.schedule != null && this.schedule.Count != 0 ?
                ViewStates.Gone : ViewStates.Visible;
            DailyScheduleChanged?.Invoke();
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context).Inflate(Resource.Layout.item_daily_schedule, viewGroup, false);
            var vh = new ScheduleViewHolder(view);
            return vh;
        }

        void SetHead(ScheduleViewHolder viewHolder, DateTime date)
        {
            viewHolder.LessonTime.SetTextColor(new Color(120, 142, 161));
            viewHolder.LessonTime.SetText(date.ToString("ddd d MMM").Replace('.', '\0'), TextView.BufferType.Normal);
        }

        void SetLessons(ScheduleViewHolder viewHolder, Schedule.Daily dailySchedule)
        {
            string res = string.Empty;
            if (dailySchedule != null && dailySchedule.Count != 0)
            {
                string title;
                int currOrder = dailySchedule[0].Order;
                for (int i = 0; i < dailySchedule.Count - 1; i++)
                {
                    if (currOrder == dailySchedule[i].Order)
                    {
                        res += currOrder + 1 + ") ";
                        currOrder++;
                    }
                    title = dailySchedule[i].Title;
                    if (title.Length > 10)
                    {
                        title = title.Substring(0, 10) + "...";
                    }
                    res += " (" + dailySchedule[i].Type + ") " + title + "\n";
                }
                if (currOrder == dailySchedule[dailySchedule.Count - 1].Order)
                {
                    res += currOrder + 1 + ") ";
                }
                title = dailySchedule[dailySchedule.Count - 1].Title;
                if (title.Length > 10)
                {
                    title = title.Substring(0, 10) + "...";
                }
                res += " (" + dailySchedule[dailySchedule.Count - 1].Type + ") " + title;
            }
            viewHolder.LessonType.SetText(res, TextView.BufferType.Normal);
        }

        void SetFirstPosDate(bool isSession)
        {
            if (!isSession)
            {
                this.FirstPosDate = DateTime.Today.AddDays(-this.ItemCount / 2);
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
                this.itemCount = 1;
            }
            else if (schedule.IsByDate)
            {
                this.itemCount = TimeSpan.FromTicks(System.Math.Abs(
                    schedule.GetSchedule(0).Day - schedule.GetSchedule(schedule.Count - 1).Day)).Days + 1;
            }
            else
            {
                this.itemCount = 200 * 2;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ScheduleViewHolder viewHolder))
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
            var dailySchedule = this.schedule.GetSchedule(date);
            SetLessons(viewHolder, dailySchedule);
            SetHead(viewHolder, date);
        }


        public class ScheduleViewHolder : RecyclerView.ViewHolder
        {
            public TextView LessonTime { get; }
            public TextView LessonType { get; }
            public LinearLayout LessonPlace { get; }
            public LinearLayout LessonLayout { get; }
            public RelativeLayout HeadLayout { get; }
            public RelativeLayout BodyLayout { get; }

            public ScheduleViewHolder(View view) : base(view)
            {
                this.LessonTime = view.FindViewById<TextView>(Resource.Id.text_schedule_time_grid);
                this.LessonType = view.FindViewById<TextView>(Resource.Id.text_schedule_grid);
                this.LessonPlace = view.FindViewById<LinearLayout>(Resource.Id.linear_layout_schedule_grid);
                this.LessonLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_schedule_grid);
                this.HeadLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule_head_grid);
                this.BodyLayout = view.FindViewById<RelativeLayout>(Resource.Id.layout_schedule_body_grid);
            }
        }
    }
}