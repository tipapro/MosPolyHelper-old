namespace MosPolytechHelper.Features.StudentTimetable
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using System;
    using System.Threading.Tasks;

    class TimetableModel
    {
        ILogger logger;
        IDownloader downloader;
        ITimetableConverter timetableConverter;

        DateTime GetFirstWeekDay(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            // Separately because DayOfWeek.Sunday == 0
            if (dayOfWeek == DayOfWeek.Sunday)
                return date.AddDays((int)DayOfWeek.Monday - 7);
            return date.AddDays((int)DayOfWeek.Monday - (int)dayOfWeek);
        }

        public FullTimetable FullTimetable { get; private set; }
        public bool IsFirstWeekEven { get; set; }

        public TimetableModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<TimetableModel>();
            this.downloader = new Downloader(loggerFactory);
            this.timetableConverter = new TimetableConverter(loggerFactory);
        }

        public async Task<string[]> GetGroupListAsync()
        {
            string serGroupList = await this.downloader.DownloadGroupListAsync();
            return await this.timetableConverter.ConvertToGroupList(serGroupList);
        }

        private async Task DownloadTimetable(string group)
        {
            string serTimetable = await this.downloader.DownloadTimetable(group);

            this.FullTimetable = await this.timetableConverter.ConvertToFullTimetableAsync(serTimetable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="date"></param>
        /// <param name="isSession"></param>
        /// <returns>Timetable for a concrete date or null if it is not founded</returns>
        public async Task GetTimetableAsync(string group, bool isSession)
        {
            if (this.FullTimetable == null || this.FullTimetable.Group.Title != group)
                await DownloadTimetable(group);
            // TODO: See below 1
            if (group != this.FullTimetable.Group.Title)
                throw new NotImplementedException();
            // TODO: See below 2
            if (isSession != this.FullTimetable.IsSession)
                throw new NotImplementedException();
        }

        public bool IsEvenWeek(DateTime date)
        {
            const int FirstDay = 213;   // 1st August (or 31st July for leap year) 
            int firstDayYear = date.Year - FirstDay / date.DayOfYear;
            var firstDayDate = new DateTime(firstDayYear, 8, 1);
            return (GetFirstWeekDay(firstDayDate) - GetFirstWeekDay(date)).Days % 2
                    != (this.IsFirstWeekEven ? 1 : 0);
        }
    }
}