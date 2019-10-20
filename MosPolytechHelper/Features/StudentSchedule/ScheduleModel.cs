namespace MosPolytechHelper.Features.StudentSchedule
{
    using MosPolytechHelper.Common;
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Domain;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    class ScheduleModel
    {
        const string CurrentExtension = ".current";
        const string OldExtension = ".old";
        const string CustomExtension = ".custom";
        const string ScheduleFolder = "cashed_schedules";

        ILogger logger;
        IDownloader downloader;
        IScheduleConverter scheduleConverter;
        ISerializer serializer;
        IDeserializer deserializer;

        async Task<Schedule> DownloadScheduleAsync(string group, bool isSession)
        {
            Schedule schedule = null;
            try
            {
                string serSchedule = await this.downloader.DownloadSchedule(group, isSession);
                schedule = await this.scheduleConverter.ConvertToScheduleAsync(serSchedule);
                try
                {
                    await SaveScheduleAsync(schedule);
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex1)
            {
                try
                {
                    Announce.Invoke("Schedule wasn't downloaded. Trying to find offline schedule");
                    var (serSchedule, time) = ReadSchedule(group);
                    try
                    {
                        schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                    }
                    catch (Exception)
                    {
                    }
                    if (schedule == null)
                    {
                        throw new Exception("ScheduleFromStorage Fail");
                    }
                    schedule.LastUpdate = DateTime.FromBinary(time);
                    Announce.Invoke("Offline schedule was founded");
                }
                catch (Exception ex2)
                {
                    Announce.Invoke("Offline schedule wasn't founded");
                    schedule = null;
                }
            }
            return schedule;
        }

        async Task<string[]> DownloadGroupListAsync()
        {
            string[] groupList;
            try
            {
                string serGroupList = await this.downloader.DownloadGroupListAsync();
                groupList = await this.scheduleConverter.ConvertToGroupList(serGroupList);
                try
                {

                    await SaveGroupListAsync(groupList);
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
                    string res = await ReadGroupListAsync();
                    groupList = await this.deserializer.DeserializeAsync<string[]>(res);
                    if (groupList.Length == 0)
                    {
                        throw new Exception("GroupListFromStorage Fail");
                    }
                    Announce.Invoke("Offline list was founded");
                }
                catch (Exception)
                {
                    Announce.Invoke("Offline list wasn't founded");
                    groupList = new string[0];
                }
            }
            return groupList;
        }

        async Task<string> ReadGroupListAsync()
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            string serGroupList = await File.ReadAllTextAsync(backingFile);
            return serGroupList;
        }

        async Task SaveGroupListAsync(string[] groupList)
        {
            string backingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "group_list");
            string serGroupList = this.serializer.Serialize(groupList);
            await File.WriteAllTextAsync(backingFile, serGroupList);
        }

        (Stream SerSchedule, long Time) ReadSchedule(string groupTitle)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ScheduleFolder, groupTitle);
            if (!Directory.Exists(folder))
            {
                return (null, 0);
            }
            var files = Directory.GetFiles(folder).Select(Path.GetFileName);
            string fileToRead = null;
            string fileToReadOld = null;
            foreach (string fileName in files)
            {
                string ext = Path.GetExtension(fileName);
                if (ext == CurrentExtension)
                {
                    fileToRead = fileName;
                }
                else if (ext == OldExtension)
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
            var strArr = Path.GetFileNameWithoutExtension(fileToRead);
            if (strArr == null)
            {
                return (null, 0);
            }
            long day = long.Parse(strArr);
            var serSchedule = File.OpenRead(Path.Combine(folder, fileToRead));
            return (serSchedule, day);
        }

        async Task SaveScheduleAsync(Schedule schedule)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ScheduleFolder, schedule.Group.Title);
            //string scheduleHash = CalculateMD5Hash(serSchedule);
            string resFileName = null;
            if (Directory.Exists(filePath))
            {
                string[] files = Directory.GetFiles(filePath);
                foreach (string fileName in files)
                {
                    if (Path.GetExtension(fileName) != CurrentExtension)
                    {
                        continue;
                    }
                    resFileName = fileName;
                }
                filePath = Path.Combine(filePath, schedule.LastUpdate.ToBinary() + CurrentExtension);
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (resFileName == null)
                {
                    foreach (string fileName in files)
                    {
                        if (Path.GetExtension(fileName) == OldExtension)
                        {
                            File.Delete(fileName);
                        }
                        if (Path.GetExtension(fileName) == CurrentExtension)
                        {
                            File.Move(fileName, Path.ChangeExtension(fileName, OldExtension));
                        }
                    }
                    this.serializer.Serialize(filePath, schedule);
                }
                else
                {
                    File.Move(resFileName, filePath);
                }
            }
            else
            {
                filePath = Path.Combine(filePath, schedule.LastUpdate.ToBinary() + CurrentExtension);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                this.serializer.Serialize(filePath, schedule);
            }
        }

        public event Action<string> Announce;

        public Schedule Schedule { get; private set; }
        public string[] GroupList { get; private set; }

        public ScheduleModel(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleModel>();
            this.downloader = new Downloader(loggerFactory);
            this.scheduleConverter = new ScheduleConverter(loggerFactory);
            var converter = new ProtoConverter();
            this.serializer = converter;
            this.deserializer = converter;
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
            if (downloadNew)
            {
                this.Schedule = await DownloadScheduleAsync(group, isSession);
            }
            else
            {
                if (this.Schedule != null && this.Schedule.Group.Title == group && this.Schedule.IsSession == isSession)
                {
                    return this.Schedule;
                }
                try
                {
                    var (serSchedule, time) = ReadSchedule(group);
                    if (serSchedule == null)
                    {
                        this.Schedule = null;
                        return null;
                    }
                    try
                    {
                        this.Schedule = await this.deserializer.DeserializeAsync<Schedule>(serSchedule);
                    }
                    catch (Exception ex)
                    {
                    }
                    if (this.Schedule == null)
                    {
                        throw new Exception("ScheduleFromStorage Fail");
                    }
                    this.Schedule.LastUpdate = DateTime.FromBinary(time);
                }
                catch (Exception)
                {
                    Announce.Invoke("Offline schedule wasn't founded");
                    this.Schedule = null;
                }
            }

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

        string CalculateMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }
    }
}