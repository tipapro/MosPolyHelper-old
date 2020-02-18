namespace MosPolyHelper.Features.Buildings
{
    using MosPolyHelper.Domains.BuildingsDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;

    class BuildingsVm : ViewModelBase
    {
        ILogger logger;
        BuildingsModel model;
        Buildings buildings;

        public Buildings Buildings
        {
            get => this.buildings;
            set => SetValue(ref this.buildings, value);
        }
        public ICommand RefreshCommand { get; }
        public ICommand InfoCommand { get; }

        public BuildingsVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator) :
            base(mediator, ViewModels.Buildings)
        {
            this.logger = loggerFactory.Create<BuildingsVm>();
            this.model = new BuildingsModel();
            this.RefreshCommand = new Command(Refresh);
            this.InfoCommand = new Command(GetInfo);
        }

        async void Refresh()
        {
            this.Buildings = await this.model.GetBuildingsAsync(true);
        }

        public async void SetUpBuildings()
        {
            this.Buildings = await this.model.GetBuildingsAsync(false);
        }

        void GetInfo()
        {

        }
    }
}