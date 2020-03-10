namespace MosPolyHelper.Features.Schedule
{
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    class ScheduleModel
    {
        const string CurrentExtension = ".current";
        const string OldExtension = ".backup";
        const string CustomExtension = ".custom";
        const string ScheduleFolder = "cached_schedules";
        const string SessionScheduleFolder = "session";
        const string RegularScheduleFolder = "regular";

        readonly ILogger logger;
        readonly IScheduleDownloader downloader;
        readonly IScheduleConverter scheduleConverter;
        readonly ISerializer serializer;
        readonly IDeserializer deserializer;

        int scheduleCounter;

        async Task<string> ReadGroupListAsync()
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            return await File.ReadAllTextAsync(backingFile);
        }

        Task SaveGroupListAsync(string[] groupList)
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            return this.serializer.SerializeAndSaveAsync(backingFile, groupList);
        }

        (Stream SerSchedule, long Time) OpenReadSchedule(string groupTitle, bool isSession)
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

        public Task SaveScheduleAsync(Schedule schedule)
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
            return this.serializer.SerializeAndSaveAsync(filePath, schedule);
        }

        Task<Schedule> DownloadScheduleAsync(string groupTitle, bool isSession)
        {
            return DownloadScheduleAsync(groupTitle, isSession, CancellationToken.None);
        }

        async Task<Schedule> DownloadScheduleAsync(string groupTitle, bool isSession, CancellationToken ct)
        {
            string serSchedule;
            try
            {
                serSchedule = await this.downloader.DownloadSchedule(groupTitle, isSession);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw ex;
                }
                return null;
            }
            ct.ThrowIfCancellationRequested();
            var schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule, Announce);
            if (schedule != null)
            {
                schedule.IsSession = isSession;
            }
            return schedule;
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string serGroupList;
            try
            {
                serGroupList = await this.downloader.DownloadGroupListAsync();
            }
            catch (WebException)
            {
                return null;
            }
            return await this.scheduleConverter.ConvertToGroupList(serGroupList);
        }

        readonly object key = new object();

        public event Action<string> Announce;
        public event Action<int> DownloadProgressChanged;

        public Schedule Schedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new ScheduleDownloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
            this.serializer = DependencyInjector.GetProtofubISerializer();
            this.deserializer = DependencyInjector.GetProtofubIDeserializer();
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
                        await SaveScheduleAsync(this.Schedule);
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
                        var (serSchedule, time) = OpenReadSchedule(group, isSession);
                        this.Schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                        if (this.Schedule == null)
                        {
                            throw new Exception("Read schedule from storage fail");
                        }
                        if (Schedule.Version != System.Environment.GetEnvironmentVariable("ScheduleVersion"))
                        {
                            throw new Exception("Read schedule from storage fail");
                        }
                        this.Schedule.LastUpdate = new DateTime(time);
                        this.Schedule.IsSession = isSession;
                        this.Schedule.SetUpGroup();
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
                    var (serSchedule, time) = OpenReadSchedule(group, isSession);
                    this.Schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                    if (this.Schedule == null)
                    {
                        throw new Exception("Read schedule from storage fail");
                    }
                    if (Schedule.Version != System.Environment.GetEnvironmentVariable("ScheduleVersion"))
                    {
                        throw new Exception("Read schedule from storage fail");
                    }
                    this.Schedule.LastUpdate = new DateTime(time);
                    this.Schedule.IsSession = isSession;
                    this.Schedule.SetUpGroup();
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
                    try
                    {
                        await SaveGroupListAsync(this.GroupList);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex, "Saving group list error");
                    }
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
            GetSchedules(IList<string> groupList, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (groupList == null || groupList.Count == 0)
                {
                    return (null, null, null, null, null);
                }
                var collection = new ConcurrentBag<Schedule>();
                var lessonTitles = new ConcurrentDictionary<string, bool>();
                var teachers = new ConcurrentDictionary<string, bool>();
                var auditoriums = new ConcurrentDictionary<string, bool>();
                var lessonTypes = new ConcurrentDictionary<string, bool>();
                var po = new ParallelOptions
                {
                    CancellationToken = ct
                };
                this.scheduleCounter = 0;
                int maxCount = groupList.Count * 3 + groupList.Count / 33;
                ct.Register(() => this.downloader.Abort());
                    Parallel.ForEach(Partitioner.Create(0, groupList.Count), po, range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            try
                            {
                                Interlocked.Increment(ref this.scheduleCounter);
                                lock (key)
                                {
                                    DownloadProgressChanged?.Invoke(this.scheduleCounter * 10000 / maxCount);
                                }
                                po.CancellationToken.ThrowIfCancellationRequested();
                                var schedule = DownloadScheduleAsync(groupList[i], false, po.CancellationToken).GetAwaiter().GetResult();
                                if (schedule != null)
                                {
                                    schedule.ToNormal();
                                    AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes, po.CancellationToken);
                                    collection.Add(schedule);
                                }
                                Interlocked.Increment(ref this.scheduleCounter);
                                lock (key)
                                {
                                    DownloadProgressChanged?.Invoke(this.scheduleCounter * 10000 / maxCount);
                                }
                                po.CancellationToken.ThrowIfCancellationRequested();
                                schedule = DownloadScheduleAsync(groupList[i], true, po.CancellationToken).GetAwaiter().GetResult();
                                if (schedule != null)
                                {
                                    schedule.ToNormal();
                                    AddAllDataFromSchedule(schedule, lessonTitles, teachers, auditoriums, lessonTypes, po.CancellationToken);
                                    collection.Add(schedule);
                                }
                                Interlocked.Increment(ref this.scheduleCounter);
                                lock (key)
                                {
                                    DownloadProgressChanged?.Invoke(this.scheduleCounter * 10000 / maxCount);
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Status == WebExceptionStatus.RequestCanceled)
                                {
                                    po.CancellationToken.ThrowIfCancellationRequested();
                                }
                            }
                        }
                    });
                int last = this.scheduleCounter * 10000 / maxCount;
                int delta = (10000 - last) / 12;
                var lessonArray = lessonTitles.Keys.ToArray();
                DownloadProgressChanged?.Invoke(last += delta);
                var teacherArray = teachers.Keys.ToArray();
                DownloadProgressChanged?.Invoke(last += delta);
                var auditoriumArray = auditoriums.Keys.ToArray();
                DownloadProgressChanged?.Invoke(last += delta);
                var typeArray = lessonTypes.Keys.ToArray();
                DownloadProgressChanged?.Invoke(last += delta);
                Array.Sort(lessonArray);
                DownloadProgressChanged?.Invoke(last += 2 * delta);
                Array.Sort(teacherArray);
                DownloadProgressChanged?.Invoke(last += 2 * delta);
                Array.Sort(auditoriumArray);
                DownloadProgressChanged?.Invoke(last += 2 * delta);
                Array.Sort(typeArray);
                DownloadProgressChanged?.Invoke(10000);
                return (collection.ToArray(), lessonArray, teacherArray, auditoriumArray, typeArray);
            }, ct);
        }

        void AddAllDataFromSchedule(Schedule schedule, ConcurrentDictionary<string, bool> lessonTitles,
            ConcurrentDictionary<string, bool> teachers, ConcurrentDictionary<string, bool> auditoriums,
            ConcurrentDictionary<string, bool> lessonTypes, CancellationToken ct)
        {
            foreach (var dayliSchedule in schedule)
            {
                foreach (var lesson in dayliSchedule)
                {
                    ct.ThrowIfCancellationRequested();
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
    }
}