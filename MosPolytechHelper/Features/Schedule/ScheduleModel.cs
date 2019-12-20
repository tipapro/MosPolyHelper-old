namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class ScheduleModel
    {
        const string CurrentExtension = ".current";
        const string OldExtension = ".backup";
        const string CustomExtension = ".custom";
        const string ScheduleFolder = "cashed_schedules";
        const string SessionScheduleFolder = "session";
        const string RegularScheduleFolder = "regular";

        readonly ILogger logger;
        readonly IScheduleDownloader downloader;
        readonly IScheduleConverter scheduleConverter;
        readonly ISerializer serializer;
        readonly IDeserializer deserializer;

        async Task<string> ReadGroupListAsync()
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            return await File.ReadAllTextAsync(backingFile);
        }

        async Task SaveGroupListAsync(string[] groupList)
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            string serGroupList = this.serializer.Serialize(groupList);
            await File.WriteAllTextAsync(backingFile, serGroupList);
        }

        (Stream SerSchedule, long Time) ReadSchedule(string groupTitle, bool isSession)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ScheduleFolder, groupTitle, isSession ? SessionScheduleFolder : RegularScheduleFolder);
            if (!Directory.Exists(folder))
            {
                return (null, 0);
            }
            var files = Directory.GetFiles(folder).Select(Path.GetFileName);
            string fileToRead = null;
            string fileToReadOld = null;
            foreach (string fileName in files)
            {
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension == CurrentExtension)
                {
                    fileToRead = fileName;
                }
                else if (fileExtension == OldExtension)
                {
                    fileToReadOld = fileName;
                }
            }
            if (fileToRead == null)
            {
                if (fileToReadOld == null)
                {
                    return (null, 0);
                }
                fileToRead = fileToReadOld;
            }
            string strArr = Path.GetFileNameWithoutExtension(fileToRead);
            if (strArr == null)
            {
                return (null, 0);
            }
            long day = long.Parse(strArr);
            var serSchedule = File.OpenRead(Path.Combine(folder, fileToRead));
            return (serSchedule, day);
        }

        void SaveScheduleAsync(Schedule schedule)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ScheduleFolder, schedule.Group.Title, schedule.IsSession ? SessionScheduleFolder : RegularScheduleFolder);
            if (Directory.Exists(filePath))
            {
                string[] files = Directory.GetFiles(filePath);
                foreach (string fileName in files)
                {
                    if (Path.GetExtension(fileName) == OldExtension)
                    {
                        File.Delete(fileName);
                    }
                    else
                    {
                        File.Move(fileName, Path.ChangeExtension(fileName, OldExtension));
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(filePath, schedule.LastUpdate.Ticks + CurrentExtension);
            this.serializer.Serialize(filePath, schedule);
        }

        async Task<Schedule> DownloadScheduleAsync(string group, bool isSession)
        {
            Schedule schedule;
            string serSchedule = await this.downloader.DownloadSchedule(group, isSession);
            schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule);
            schedule.IsSession = isSession;
            try
            {
                SaveScheduleAsync(schedule);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Saving schedule error");
            }
            return schedule;
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string[] groupList;
            string serGroupList = await this.downloader.DownloadGroupListAsync();
            groupList = await this.scheduleConverter.ConvertToGroupList(serGroupList);
            try
            {
                await SaveGroupListAsync(groupList);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Saving group list error");
            }
            return groupList;
        }

        public event Action<string> Announce;

        public Schedule Schedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new ScheduleDownloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
            this.serializer = DependencyInjector.GetISerializer();
            this.deserializer = DependencyInjector.GetIDeserializer();
        }

        public async Task<Schedule> GetScheduleAsync(string group, bool isSession, bool downloadNew, Schedule.Filter scheduleFilter)
        {
            if (downloadNew)
            {
                try
                {
                    this.Schedule = await DownloadScheduleAsync(group, isSession);
                }
                catch (Exception ex1)
                {
                    this.logger.Error(ex1, "Download schedule error");
                    try
                    {
                        Announce?.Invoke(StringProvider.GetString(StringId.ScheduleWasntFounded));
                        var (serSchedule, time) = ReadSchedule(group, isSession);
                        this.Schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                        if (this.Schedule == null)
                        {
                            throw new Exception("Read schedule from storage fail");
                        }
                        this.Schedule.LastUpdate = new DateTime(time);
                        this.Schedule.IsSession = isSession;
                        Announce.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasFounded));
                    }
                    catch (Exception ex2)
                    {
                        this.logger.Error(ex2, "Read schedule after download failed error");
                        Announce?.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasntFounded));
                        this.Schedule = null;
                    }
                }
            }
            else
            {
                if (this.Schedule != null && this.Schedule.Group.Title == group && this.Schedule.IsSession == isSession)
                {
                    return this.Schedule;
                }
                try
                {
                    var (serSchedule, time) = ReadSchedule(group, isSession);
                    this.Schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                    if (this.Schedule == null)
                    {
                        throw new Exception("Read schedule from storage fail");
                    }
                    this.Schedule.LastUpdate = new DateTime(time);
                    this.Schedule.IsSession = isSession;
                }
                catch (Exception ex1)
                {
                    this.logger.Error(ex1, "Read schedule error");
                    Announce?.Invoke(StringProvider.GetString(StringId.OfflineScheduleWasntFounded));
                    this.Schedule = null;
                }
            }

            if (this.Schedule == null)
            {
                return null;
            }
            if (group != this.Schedule?.Group?.Title)
            {
                this.logger.Warn("{group} != {scheduleGroupTitle}", group, this.Schedule?.Group?.Title);
            }
            this.Schedule.ScheduleFilter = scheduleFilter;
            return this.Schedule;
        }

        public async Task<string[]> GetGroupListAsync(bool downloadNew)
        {
            if (downloadNew)
            {
                try
                {
                    this.GroupList = await DownloadGroupListAsync();
                }
                catch (Exception ex1)
                {
                    this.logger.Error(ex1, "Download group list error");
                    try
                    {
                        Announce?.Invoke(StringProvider.GetString(StringId.GroupListWasntFounded));
                        string serGroupList = await ReadGroupListAsync();
                        this.GroupList = await this.deserializer.DeserializeAsync<string[]>(serGroupList);
                        if (this.GroupList.Length == 0)
                        {
                            throw new Exception("Read group list from storage fail");
                        }
                        Announce.Invoke(StringProvider.GetString(StringId.OfflineGroupListWasFounded));
                    }
                    catch (Exception ex2)
                    {
                        this.logger.Error(ex2, "Read group lsit after download failed error");
                        Announce?.Invoke(StringProvider.GetString(StringId.OfflineGroupListWasntFounded));
                        this.GroupList = new string[0];
                    }
                }
            }
            return this.GroupList;
        }
    }
}