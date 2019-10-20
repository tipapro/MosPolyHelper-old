namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    class Downloader : IDownloader
    {
        ILogger logger;
        CookieContainer cookieContainer;

        async Task GetCookiesAsync()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://rasp.dmami.ru");
            request.Referer = "https://rasp.dmami.ru";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Host = "rasp.dmami.ru";
            string serializedObj = "";
            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {

                using (var reader = new StreamReader(response.GetResponseStream()))
                    serializedObj = await reader.ReadToEndAsync();
            }

            var regex = new Regex("cookie=\".*?;"); // TODO: More effective algorithm
            var matches = regex.Matches(serializedObj);
            if (matches.Count == 0)
            {
                this.logger.Warn("Cookies were not founded {serializedObj}", serializedObj);
                return;
            }
            string cookie = matches[0].Value;
            string[] str = cookie.Substring("cookie=\"".Length, cookie.Length - "cookie=\"".Length - 1)
                .Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (str.Length < 2)
                throw new Exception("Cookies ex");

            this.cookieContainer = new CookieContainer();
            this.cookieContainer.Add(new Cookie(str[0], str[1], "/", request.Host));
        }

        public Downloader(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<Downloader>();
        }

        public async Task<string> DownloadSchedule(string group, bool isSession)
        {
            if (this.cookieContainer == null)
                await GetCookiesAsync();
            this.logger.Debug("Request to download the schedule for {group} group", group);
            if (string.IsNullOrEmpty(group))
                throw new ArgumentNullException("groupName");
            var uri = new UriBuilder($"https://rasp.dmami.ru/site/group?group={group}&session=" + (isSession ? 1 : 0)).Uri;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = this.cookieContainer;
            request.Referer = uri.Scheme + uri.Host;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            string serializedObj;
            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                    serializedObj = await reader.ReadToEndAsync();
            }
            this.logger.Debug("Schedule was downloaded successfully for {group} group", group);
            return serializedObj;
        }

        public async Task<string> DownloadGroupListAsync()
        {
            if (this.cookieContainer == null)
                await GetCookiesAsync();
            var uri = new UriBuilder("https://rasp.dmami.ru/groups-list.json").Uri;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Referer = "https://rasp.dmami.ru/groups-list.json";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.CookieContainer = this.cookieContainer;
            request.Host = "rasp.dmami.ru";
            string serializedObj = "";
            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                    serializedObj = await reader.ReadToEndAsync();
            }
            return serializedObj;
        }



        /// <summary>
        /// Checks the subject for subgroup
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns>Subgroup or null if subgroup wasn't founded</returns>
        private string GetLessonSubgroup(string subjectName)
        {
            char[] charArray = subjectName.ToCharArray();
            for (int i = charArray.Length - 1; i >= 2; i--)
            {
                // Находим сочетание "...п/г..."
                if (charArray[i] != 'г' || charArray[i - 1] != '/' || charArray[i - 2] != 'п')
                    continue;
                // Находим номер группы "...п/г 123 ..."
                var resCharArr = new List<char>(10);
                bool flag = false;
                for (int j = i + 1; !flag && j < charArray.Length - 1; j++)
                {
                    if (char.IsWhiteSpace(charArray[j]))
                        continue;
                    flag = true;
                    resCharArr.Add(charArray[j]);
                    continue;
                }
                return new string(resCharArr.ToArray());
            }
            return null;
        }
    }
}