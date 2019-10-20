namespace MosPolytechHelper.Adapters
{
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Domain;
    using System;
    using System.IO;

    public class RecyclerScheduleManagerAdapter : RecyclerView.Adapter
    {
        TextView nullMessage;
        string[] path;

        public override int ItemCount =>
            this.path?.Length ?? 0;

        public RecyclerScheduleManagerAdapter(TextView nullMessage, params string[] pathes)
        {
            this.path = pathes;
            this.nullMessage = nullMessage;
            this.nullMessage.Visibility = this.path.Length == 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        public void BuildSchedule(params string[] pathes)
        {
            this.path = pathes;
            this.nullMessage.Visibility = this.path.Length == 0 ? ViewStates.Invisible : ViewStates.Visible;
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.item_schedule, viewGroup, false);
            view.Enabled = false;
            return new ScheduleManagerViewHolder(view);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            //var lesson = this.dailySchedule?[position];
            //if (lesson == null)
            {
                return;
            }
            var viewHolder = vh as ScheduleManagerViewHolder;

            //viewHolder.LessonModuleAndWeekType.SetText(moduleAndWeelType, TextView.BufferType.Normal);
        }

        public class ScheduleManagerViewHolder : RecyclerView.ViewHolder
        {

            public ScheduleManagerViewHolder(View view) : base(view)
            {
            }
        }
    }
}