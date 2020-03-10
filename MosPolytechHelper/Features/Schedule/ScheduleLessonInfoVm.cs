namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Threading.Tasks;

    class ScheduleLessonInfoVm : ViewModelBase
    {
        void HandleMessage(VmMessage message)
        {
            if (message.Count == 3)
            {
                if (message[0] is string propName)
                {
                    switch (propName)
                    {
                        case "LessonInfo" when message[1] is Lesson lesson && message[2] is DateTime date:
                            this.Lesson = lesson;
                            this.Date = date;
                            break;
                    }
                }
            }
        }


        public ScheduleLessonInfoVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.ScheduleLessonInfo)
        {
            this.Subscribe(HandleMessage);
        }

        public Lesson Lesson { get; set; }
        public DateTime Date { get; set; }

        public void ResaveSchedule()
        {
            Send(ViewModels.Schedule, "ResaveSchedule");
        }
    }
}