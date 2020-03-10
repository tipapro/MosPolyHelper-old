namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Domains.ScheduleDomain;
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities.Interfaces;
    using System;

    class ScheduleCalendarVm : ViewModelBase
    {
        void HandleMessage(VmMessage message)
        {
            if (message.Count == 5)
            {
                if (message[0] is string propName)
                {
                    switch (propName)
                    {
                        case "CalendarMode" when message[1] is Schedule schedule && message[2] is DateTime date &&
                        message[3] is Schedule.Filter filter && message[4] is bool isAdvancedSearch:
                            this.Schedule = schedule;
                            this.Date = date;
                            this.ScheduleFilter = filter;
                            this.IsAdvancedSearch = isAdvancedSearch;
                            break;
                    }
                }
            }
        }


        public ScheduleCalendarVm(ILoggerFactory loggerFactory, IMediator<ViewModels, VmMessage> mediator)
            : base(mediator, ViewModels.ScheduleCalendar)
        {
            Subscribe(HandleMessage);
        }

        public Schedule Schedule { get; set; }
        public DateTime Date { get; set; }
        public Schedule.Filter ScheduleFilter { get; set; }
        public bool IsAdvancedSearch { get; set; }

        public void DateChanged()
        {
            Send(ViewModels.Schedule, "ChangeDate", this.Date);
        }
    }
}