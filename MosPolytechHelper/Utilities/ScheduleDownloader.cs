namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    class ScheduleDownloader : IScheduleDownloader
    {
        readonly ILogger logger;
        CookieContainer cookieContainer;

        async Task GetCookiesAsync()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://rasp.dmami.ru");
            AbortRequest += request.Abort;
            try
            {
                request.Referer = "https://rasp.dmami.ru";
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Host = "rasp.dmami.ru";
                string serializedObj = "";
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    serializedObj = await reader.ReadToEndAsync();
                }

                var regex = new Regex("cookie=\".*?;"); // TODO: More effective algorithm
                var matches = regex.Matches(serializedObj);
                if (matches.Count == 0)
                {
                    this.logger.Warn($"Cookies were not founded {nameof(serializedObj)}", serializedObj);
                    return;
                }
                string cookie = matches[0].Value;
                string[] str = cookie.Substring("cookie=\"".Length, cookie.Length - "cookie=\"".Length - 1)
                    .Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (str.Length < 2)
                {
                    this.logger.Warn($"Cookies were not founded {nameof(serializedObj)}", serializedObj);
                    return;
                }

                this.cookieContainer = new CookieContainer();
                this.cookieContainer.Add(new Cookie(str[0], str[1], "/", request.Host));
                this.logger.Debug("Cookies was founded");
            }
            finally
            {
                AbortRequest -= request.Abort;
            }
        }

        Action abortRequest;
        readonly object key = new object();
        event Action AbortRequest {
            add
            {
                lock (key)
                {
                    abortRequest += value;
                }
            }
            remove
            {
                lock (key)
                {
                    abortRequest -= value;
                }
            }
        }

        public void Abort()
        {
            lock (key)
            {
                abortRequest?.Invoke();
                abortRequest = null;
            }
        }

        public ScheduleDownloader(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.Create<ScheduleDownloader>();
        }

        public async Task<string> DownloadSchedule(string groupTitle, bool isSession)
        {
            if (this.cookieContainer == null)
            {
                await GetCookiesAsync();
            }
            if (string.IsNullOrEmpty(groupTitle))
            {
                throw new ArgumentNullException(nameof(groupTitle));
            }
            var uri = new UriBuilder($"https://rasp.dmami.ru/site/group?group={groupTitle}&session=" + (isSession ? 1 : 0)).Uri;
            var request = (HttpWebRequest)WebRequest.Create(uri);

            AbortRequest += request.Abort;
            try
            {
                request.CookieContainer = this.cookieContainer;
                request.Referer = uri.Scheme + uri.Host;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                string serializedObj;
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    serializedObj = await reader.ReadToEndAsync();
                }
                this.logger.Debug($"Schedule was downloaded successfully for {nameof(groupTitle)} group", groupTitle);
                return serializedObj;
            }
            finally
            {
                AbortRequest -= request.Abort;
            }
        }

        public async Task<string> DownloadGroupListAsync()
        {
            if (this.cookieContainer == null)
            {
                await GetCookiesAsync();
            }
            var uri = new UriBuilder("https://rasp.dmami.ru/groups-list.json").Uri;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = this.cookieContainer;
            request.Referer = uri.Scheme + uri.Host;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            string serializedObj;
            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                serializedObj = await reader.ReadToEndAsync();
            }
            this.logger.Debug($"Group list was downloaded successfully");
            return serializedObj;
        }
    }
}