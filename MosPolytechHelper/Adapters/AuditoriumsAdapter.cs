namespace MosPolytechHelper.Adapters
{
    using Android.Graphics;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using MosPolytechHelper.Domain;

    public class AuditoriumsAdapter : RecyclerView.Adapter
    {
        Auditorium[] auditoriums;
        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class AuditoriumsViewHolder : RecyclerView.ViewHolder
        {
            public TextView TextAuditorium { get; }

            public AuditoriumsViewHolder(View view) : base(view)
            {
                this.TextAuditorium = view.FindViewById<TextView>(Resource.Id.text_auditorium);
            }
        }

        // Initialize the dataset of the Adapter
        public AuditoriumsAdapter(Auditorium[] auditoriums)
        {
            this.auditoriums = auditoriums;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            var view = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.item_auditorium, viewGroup, false);

            return new AuditoriumsViewHolder(view);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var aud = this.auditoriums[position];
            (viewHolder as AuditoriumsViewHolder).TextAuditorium.SetTextColor(Color.ParseColor(aud.Color));
            (viewHolder as AuditoriumsViewHolder).TextAuditorium.SetText(aud.Name, TextView.BufferType.Normal);
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get => this.auditoriums?.Length ?? 0;
        }
    }
}