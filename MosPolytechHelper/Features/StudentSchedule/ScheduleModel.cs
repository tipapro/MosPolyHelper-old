namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using System;
    using System.Threading.Tasks;

    class ScheduleModel
    {
        ILogger logger;
        IDownloader downloader;
        IScheduleConverter scheduleConverter;

        async Task<Schedule> DownloadScheduleAsync(string group)
        {
            string serSchedule = await this.downloader.DownloadSchedule(group);
            return await this.scheduleConverter.ConvertToFullScheduleAsync(serSchedule);
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string serGroupList = await this.downloader.DownloadGroupListAsync();
            return await this.scheduleConverter.ConvertToGroupList(serGroupList);
        }

        public Schedule Schedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new Downloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="date"></param>
        /// <param name="isSession"></param>
        /// <returns>Schedule for a concrete date or null if it is not founded</returns>
        public async Task<Schedule> GetScheduleAsync(string group, bool isSession, bool downloadNew, Schedule.Filter scheduleFilter)
        {
            if (this.Schedule == null || this.Schedule.Group.Title != group || downloadNew)
                this.Schedule = await DownloadScheduleAsync(group);
            // TODO: See below 1
            if (group != this.Schedule.Group.Title)
                throw new NotImplementedException("(group != this.Schedule.Group.Title)");
            // TODO: See below 2
            if (isSession != this.Schedule.IsSession)
                throw new NotImplementedException("(isSession != this.Schedule.IsSession)");
            this.Schedule.ScheduleFilter = scheduleFilter;
            return this.Schedule;
        }

        public async Task<string[]> GetGroupListAsync(bool downloadNew)
        {
            if (downloadNew)
            {
                this.GroupList = await DownloadGroupListAsync();
            }
            return this.GroupList;
        }
    }
}