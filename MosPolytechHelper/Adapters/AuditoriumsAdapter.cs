namespace MosPolytechHelper.Adapters
{
    using Android.Content.Res;
    using Android.Graphics;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Domain;

    public class AuditoriumsAdapter : RecyclerView.Adapter
    {
        readonly Auditorium[] auditoriums;

        public bool Enabled { get; set; }

        public class AuditoriumsViewHolder : RecyclerView.ViewHolder
        {
            public TextView TextAuditorium { get; }

            public AuditoriumsViewHolder(View view) : base(view)
            {
                this.TextAuditorium = view.FindViewById<TextView>(Resource.Id.text_auditorium);
            }
        }

        public AuditoriumsAdapter(Auditorium[] auditoriums)
        {
            this.Enabled = true;
            this.auditoriums = auditoriums;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.item_auditorium, viewGroup, false);

            return new AuditoriumsViewHolder(view);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            var viewHolder = vh as AuditoriumsViewHolder;
            var aud = this.auditoriums[position];
            
            if (this.Enabled)
            {
                var color = Color.ParseColor(aud.Color);
                viewHolder.TextAuditorium.SetTextColor(color);
            }
            else
            {
                viewHolder.TextAuditorium.SetTextColor(new Color(225, 225, 225));
                viewHolder.TextAuditorium.Enabled = this.Enabled;
            }

            (vh as AuditoriumsViewHolder).TextAuditorium.SetText(aud.Name, TextView.BufferType.Normal);
        }

        public override int ItemCount
        {
            get => this.auditoriums?.Length ?? 0;
        }
    }
}