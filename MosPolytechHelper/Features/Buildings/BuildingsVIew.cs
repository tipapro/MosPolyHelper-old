namespace MosPolyHelper.Features.Buildings
{
    using Android.Content;
    using Android.OS;
    using Android.Views;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using AndroidX.DrawerLayout.Widget;
    using AndroidX.RecyclerView.Widget;
    using MosPolyHelper.Adapters;
    using MosPolyHelper.Domains.BuildingsDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Features.Main;
    using MosPolyHelper.Utilities;
    using System.ComponentModel;

    class BuildingsView : FragmentBase
    {
        BuildingsVm viewModel;
        RecyclerView recyclerView;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.Buildings):
                    SetUpBuildings(this.viewModel.Buildings);
                    break;
                default:
                    break;
            }
        }

        void SetUpBuildings(Buildings buildings)
        {
            this.recyclerView.SetAdapter(new BuildingsAdapter(buildings));
        }

        public BuildingsView() : base(Fragments.Buildings)
        {
            var loggerFactory = DependencyInjector.GetILoggerFactory();
            this.viewModel = new BuildingsVm(loggerFactory, DependencyInjector.GetIMediator());
            this.viewModel.PropertyChanged += OnPropertyChanged;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);

            if (toolbar != null)
            {
                (this.Activity as MainView)?.SetSupportActionBar(toolbar);
            }
            (this.Activity as MainView).SupportActionBar.SetDisplayShowTitleEnabled(false);
            var drawer = this.Activity.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this.Activity, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            toggle.DrawerIndicatorEnabled = true;
            drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.menu_buildings, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.buildings_update)
            {
                this.viewModel.RefreshCommand.Execute(null);
            }
            else if (id == Resource.Id.buildings_info)
            {

            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.HasOptionsMenu = true;
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            this.viewModel.SetUpBuildings();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_buildings, container, false);
            this.recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_buildings);
            this.recyclerView.SetLayoutManager(new LinearLayoutManager(container.Context));
            float scale = view.Context.Resources.DisplayMetrics.Density;
            this.recyclerView.AddItemDecoration(new BuildingsAdapter.ItemDecoration((int)(4 * scale + 0.5f)));
            return view;
        }

        protected override void Dispose(bool disposing)
        {
            this.viewModel.PropertyChanged += OnPropertyChanged;
            base.Dispose(disposing);
        }
    }
}