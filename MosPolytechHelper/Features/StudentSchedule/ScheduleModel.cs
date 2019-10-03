namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class ScheduleModel
    {
        ILogger logger;
        IDownloader downloader;
        IScheduleConverter scheduleConverter;
        ISerializer serializer;
        IDeserializer deserializer;

        async Task<Schedule> DownloadScheduleAsync(string group, bool isSession)
        {
            Schedule schedule;
            try
            {
                string serSchedule = await this.downloader.DownloadSchedule(group, isSession);
                schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule);
                try
                {
                    await SaveScheduleAsync(schedule);
                }
                catch (Exception ex1)
                {

                }
            }
            catch (Exception ex2)
            {
                try
                {
                    Announce.Invoke("Schedule wasn't downloaded. Trying to find offline schedule");
                    schedule = await ReadScheduleAsync(group);
                    Announce.Invoke("Offline schedule was founded");
                }
                catch (Exception ex3)
                {
                    Announce.Invoke("Offline schedule wasn't founded");
                    schedule = null;
                }
            }
            return schedule;
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string[] res;
            try
            {
                string serGroupList = await this.downloader.DownloadGroupListAsync();
                res = await this.scheduleConverter.ConvertToGroupList(serGroupList);
                try
                {
                    
                    await SaveGroupListAsync(res);
                }
                catch (Exception)
                {
                    
                }
            }
            catch (Exception)
            {
                try
                {
                    Announce.Invoke("Group list wasn't downloaded. Trying to find offline list");
                    res = await ReadGroupListAsync();
                    Announce.Invoke("Offline list was founded");
                }
                catch (Exception)
                {
                    Announce.Invoke("Offline list wasn't founded");
                    res = new string[0];
                }
            }
            return res;
        }

        async Task<string[]> ReadGroupListAsync()
        {
            var backingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "group_list");
            var serGroupList = await File.ReadAllTextAsync(backingFile);
            var groupList = deserializer.Deserialize<string[]>(serGroupList);
            if (groupList.Length == 0)
            {
                throw new Exception("GroupListFromStorege Fail");
            }
            return groupList;
        }

        async Task SaveGroupListAsync(string[] groupList)
        {
            var backingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "group_list");
            var serGroupList = this.serializer.Serialize(groupList);
            await File.WriteAllTextAsync(backingFile, serGroupList);
        }

        async Task<Schedule> ReadScheduleAsync(string groupTitle)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                groupTitle);
            if (!Directory.Exists(folder))
            {
                return null;
            }
            var files = Directory.GetFiles(folder).Select(file => Path.GetFileNameWithoutExtension(file));
            long fileToRead = 0;
            foreach (var fileName in files)
            {
                long name = long.Parse(fileName);
                if (name > fileToRead)
                {
                    fileToRead = name;
                }
            }
            if (fileToRead != 0)
            {
                var serSchedule = await File.ReadAllTextAsync(Path.Combine(folder, fileToRead.ToString()));
                Schedule schedule = null;
                try
                {
                    schedule = deserializer.Deserialize<Schedule>(serSchedule);
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    int H = 4;
                }
                schedule.LastUpdate = new DateTime(fileToRead);
                return schedule;
            }
            else
            {
                return null;
            }
        }

        async Task SaveScheduleAsync(Schedule schedule)
        {
            var backingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                schedule.Group.Title);
            var serSchedule = this.serializer.Serialize(schedule);
            Directory.CreateDirectory(backingFile);
            backingFile = Path.Combine(backingFile, schedule.LastUpdate.ToFileTime().ToString());
            await File.WriteAllTextAsync(backingFile, serSchedule);
        }

        public event Action<string> Announce;

        public Schedule Schedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new Downloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
            var jsonConverter = new JsonConverter();
            this.serializer = jsonConverter;
            this.deserializer = jsonConverter;
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
                this.Schedule = await DownloadScheduleAsync(group, isSession);
            if (this.Schedule == null)
                return null;
            // TODO: See below 1
            //if (group != this.Schedule.Group.Title)
            //    throw new Exception("(group != this.Schedule.Group.Title)");
            // TODO: See below 2
            //if (isSession != this.Schedule.IsSession)
            //    throw new Exception("(isSession != this.Schedule.IsSession)");
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