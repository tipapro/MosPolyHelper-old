namespace MosPolyHelper.Adapters
{
    using Android.Graphics;
    using Android.Text;
    using Android.Text.Style;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;
    using System.Globalization;
    using System.Linq;

    public class DailyShceduleGridAdapter : RecyclerView.Adapter
    {
        Schedule schedule;
        Schedule.Filter scheduleFilter;
        CultureInfo customFormat;
        int itemCount;
        Color colorParagraph;
        Color colorTimeBackground;
        Color colorTitle;
        Color colorCurrentTitle;

        readonly Color[] lessonTimeColors = new Color[]
        {
            new Color(239, 83, 80),   // Red
            new Color(255, 160, 0),   // Orange
            new Color(76, 175, 80),   // Green
            new Color(66, 165, 245),   // Blue
            new Color(57, 73, 171),   // Indigo
            new Color(106, 27, 154),   // Purple
            new Color(55, 71, 79),   // Gray
            new Color(206, 221, 235)    // One color mode
        };

        readonly Color[] lessonTypeColors = new Color[]
        {
            new Color(235, 65, 65),     // Exam, Credit
            new Color(41, 182, 246)   // Other
        };

        public override int ItemCount => this.itemCount;
        public event Action<DateTime> ItemClick;
        public bool ShowGroup { get; set; }

        public DailyShceduleGridAdapter(Schedule schedule, Schedule.Filter scheduleFilter, bool showGroup,
            Color colorParagraph, Color colorTimeBackground, Color colorTitle, Color colorCurrentTitle)
        {
            this.scheduleFilter = scheduleFilter;
            this.schedule = schedule;
            this.colorTitle = colorTitle;
            this.colorCurrentTitle = colorCurrentTitle;
            this.ShowGroup = showGroup;
            SetCount(schedule);
            if (schedule != null)
            {
                SetFirstPosDate(schedule);
            }

            this.colorParagraph = colorParagraph;
            this.colorTimeBackground = colorTimeBackground;

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
                using var res = new SpannableStringBuilder();
                viewHolder.LessonTime.SetTextColor(colorTitle);
                if (date == DateTime.Today)
                {
                    viewHolder.LessonTime.SetTextColor(colorCurrentTitle);
                }
                viewHolder.LessonTime.SetText(date.ToString("ddd, d MMM", this.customFormat).Replace('.', '\0'), TextView.BufferType.Normal);
            }

            void SetLessons(ViewHolder viewHolder, Schedule.Daily dailySchedule, DateTime date)
        {
            using var res = new SpannableStringBuilder();

            if (dailySchedule != null && dailySchedule.Count != 0)
            {
                string title;
                var time = dailySchedule[0].GetTime(date);
                SpansAppend(res, (dailySchedule[0].Order + 1) + ") " + time.StartTime + "-" + time.EndTime,
                    SpanTypes.ExclusiveExclusive,
                    new QuoteSpan(colorParagraph),
                    new BackgroundColorSpan(colorTimeBackground),
                    new TypefaceSpan("sans-serif-medium"));

                if (this.ShowGroup)
                {
                    SpansAppend(res, "\n" + dailySchedule[0].Type.ToUpper() + "  " +
                        dailySchedule[0].Group.Title, SpanTypes.ExclusiveExclusive,
                    new ForegroundColorSpan(dailySchedule[0].IsImportant() ? this.lessonTypeColors[0] : this.lessonTypeColors[1]),
                    new RelativeSizeSpan(0.8f), new TypefaceSpan("sans-serif-medium"));
                }
                else
                {
                    SpansAppend(res, "\n" + dailySchedule[0].Type.ToUpper(), SpanTypes.ExclusiveExclusive,
                    new ForegroundColorSpan(dailySchedule[0].IsImportant() ? this.lessonTypeColors[0] : this.lessonTypeColors[1]),
                    new RelativeSizeSpan(0.8f), new TypefaceSpan("sans-serif-medium"));
                }

                title = dailySchedule[0].Title;
                res.Append("\n" + title);


                for (int i = 1; i < dailySchedule.Count; i++)
                {
                    if (!dailySchedule[i].EqualsTime(dailySchedule[i - 1], date))
                    {
                        time = dailySchedule[i].GetTime(date);
                        SpansAppend(res, "\n" + (dailySchedule[i].Order + 1) + ") " + time.StartTime + "-" + time.EndTime,
                            SpanTypes.ExclusiveExclusive,
                            new QuoteSpan(colorParagraph),
                            new BackgroundColorSpan(colorTimeBackground),
                            new TypefaceSpan("sans-serif-medium"));
                    }
                    if (this.ShowGroup)
                    {
                        SpansAppend(res, "\n" + dailySchedule[i].Type.ToUpper() + "  " +
                            dailySchedule[i].Group.Title, SpanTypes.ExclusiveExclusive,
                        new ForegroundColorSpan(dailySchedule[i].IsImportant() ? this.lessonTypeColors[0] : this.lessonTypeColors[1]),
                        new RelativeSizeSpan(0.8f), new TypefaceSpan("sans-serif-medium"));
                    }
                    else
                    {
                        SpansAppend(res, "\n" + dailySchedule[i].Type.ToUpper(), SpanTypes.ExclusiveExclusive,
                        new ForegroundColorSpan(dailySchedule[i].IsImportant() ? this.lessonTypeColors[0] : this.lessonTypeColors[1]),
                        new RelativeSizeSpan(0.8f), new TypefaceSpan("sans-serif-medium"));
                    }
                    title = dailySchedule[i].Title;
                    res.Append("\n" + title);
                }
            }
            viewHolder.LessonType.SetText(res, TextView.BufferType.Normal);
        }

        void SpansAppend(SpannableStringBuilder builder, string text, SpanTypes flags,
            params Java.Lang.Object[] spans)
        {
            int start = builder.Length();
            builder.Append(text);
            int length = builder.Length();
            foreach (var span in spans)
            {
                builder.SetSpan(span, start, length, flags);
            }
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
                if (this.itemCount > 400 || this.itemCount < 0)
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