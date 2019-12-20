namespace MosPolyHelper.Features
{
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Features.Common;

    public class MainVm : ViewModelBase
    {
        public MainVm(IMediator<ViewModels, VmMessage> mediator) : base(mediator, ViewModels.Main)
        {

        }

        public void ChangeShowEmptyLessons(bool showEmptyLessons)
        {
            Send(ViewModels.Schedule, "ShowEmptyLessons", showEmptyLessons);
        }
        public void ChangeShowColoredLessons(bool showColoredLessons)
        {
            Send(ViewModels.Schedule, "ShowColoredLessons", showColoredLessons);
        }
    }
}