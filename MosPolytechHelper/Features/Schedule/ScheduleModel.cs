namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Common;
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Domain;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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

        async Task<Schedule> DownloadScheduleAsync(string groupTitle, bool isSession)
        {
            string serSchedule;
            try
            {
                System.Diagnostics.Debug.WriteLine("Download " + groupTitle + " Normal");
                serSchedule = await this.downloader.DownloadSchedule(groupTitle, isSession);
            }
            catch (Exception)
            {
                return null;
            }
            System.Diagnostics.Debug.WriteLine("Convert " + groupTitle + " Normal");
            var schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule);
            if (schedule != null)
            {
                schedule.IsSession = isSession;
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

        public async Task<Schedule> GetScheduleAsync(string group, bool isSession, bool downloadNew)
        {
            if (downloadNew)
            {
                try
                {
                    this.Schedule = await DownloadScheduleAsync(group, isSession);
                    try
                    {
                        SaveScheduleAsync(this.Schedule);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex, "Saving schedule error");
                    }
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

        public async Task<(Schedule[] Schedules, string[] LessonTitles, string[] Teachers, string[] Auditoriums, string[] LessonTypes)> 
            GetSchedules(string[] groupList)
        {
            return await Task.Run(() =>
            {
                if (groupList == null || groupList.Length == 0)
                {
                    return (null, null, null, null, null);
                }
                var collection = new ConcurrentBag<Schedule>();
                var lessonTitles = new ConcurrentDictionary<string, bool>();
                var teachers = new ConcurrentDictionary<string, bool>();
                var auditoriums = new ConcurrentDictionary<string, bool>();
                var lessonTypes = new ConcurrentDictionary<string, bool>();
                Parallel.ForEach(Partitioner.Create(0, groupList.Length), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        System.Diagnostics.Debug.WriteLine(i + " Normal");
                        var schedule = DownloadScheduleAsync(groupList[i], false).GetAwaiter().GetResult();
                        if (schedule != null)
                        {
                            AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes);
                            collection.Add(schedule);
                        }
                        System.Diagnostics.Debug.WriteLine(i + " Session");
                        schedule = DownloadScheduleAsync(groupList[i], true).GetAwaiter().GetResult();
                        if (schedule != null)
                        {
                            schedule.ToNormal();
                            AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes);
                            collection.Add(schedule);
                        }
                    }
                });
                return (collection.ToArray(), lessonTitles.Keys.ToArray(), teachers.Keys.ToArray(), 
                auditoriums.Keys.ToArray(), lessonTypes.Keys.ToArray());
            });
        }

        void AddAllDataFromSchedule(Schedule schedule, ConcurrentDictionary<string, bool> lessonTitles,
            ConcurrentDictionary<string, bool> teachers, ConcurrentDictionary<string, bool> auditoriums,
            ConcurrentDictionary<string, bool> lessonTypes)
        {
            foreach (var dayliSchedule in schedule)
            {
                foreach (var lesson in dayliSchedule)
                {
                    lessonTitles.TryAdd(lesson.Title, false);
                    foreach (var teacher in lesson.Teachers)
                    {
                        teachers.TryAdd(teacher.GetFullName(), false);
                    }
                    foreach (var auditorium in lesson.Auditoriums)
                    {
                        auditoriums.TryAdd(auditorium.Name, false);
                    }
                    lessonTypes.TryAdd(lesson.Type, false);
                }
            }
        }

        //public async Task<(Schedule[] Schedules, string[] LessonTitles, string[] Teachers, string[] Auditoriums, string[] LessonTypes)>
        //    GetSchedules(string[] groupList)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var collection = new List<Schedule>();
        //        var lessonTitles = new List<string>();
        //        var teachers = new List<string>();
        //        var auditoriums = new List<string>();
        //        var lessonTypes = new List<string>();
        //        foreach (var groupTitle in groupList)
        //        {
        //            System.Diagnostics.Debug.WriteLine(groupTitle + " Normal");
        //            var schedule = DownloadScheduleAsync(groupTitle, false).GetAwaiter().GetResult();
        //            if (schedule != null)
        //            {
        //                AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes);
        //                collection.Add(schedule);
        //            }
        //            System.Diagnostics.Debug.WriteLine(groupTitle + " Session");
        //            schedule = DownloadScheduleAsync(groupTitle, true).GetAwaiter().GetResult();
        //            if (schedule != null)
        //            {
        //                schedule.ToNormal();
        //                AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes);
        //                collection.Add(schedule);
        //            }
        //        }
        //        return (collection.ToArray(), lessonTitles.ToArray(), teachers.ToArray(),
        //        auditoriums.ToArray(), lessonTypes.ToArray());
        //    });
        //}

        //void AddAllDataFromSchedule(Schedule schedule, List<string> lessonTitles,
        //    List<string> teachers, List<string> auditoriums,
        //    List<string> lessonTypes)
        //{
        //    foreach (var dayliSchedule in schedule)
        //    {
        //        foreach (var lesson in dayliSchedule)
        //        {
        //            if (!lessonTitles.Contains(lesson.Title))
        //                lessonTitles.Add(lesson.Title);
        //            foreach (var teacher in lesson.Teachers)
        //            {
        //                if (!teachers.Contains(teacher.GetFullName()))
        //                    teachers.Add(teacher.GetFullName());
        //            }
        //            foreach (var auditorium in lesson.Auditoriums)
        //            {
        //                if (!auditoriums.Contains(auditorium.Name))
        //                    auditoriums.Add(auditorium.Name);
        //            }
        //            if (!lessonTypes.Contains(lesson.Type))
        //                lessonTypes.Add(lesson.Type);
        //        }
        //    }
        //}
    }
}