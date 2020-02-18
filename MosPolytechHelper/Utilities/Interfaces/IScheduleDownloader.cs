namespace MosPolyHelper.Utilities.Interfaces
{
    using System.Net;
    using System.Threading.Tasks;

    public interface IScheduleDownloader
    {
        void Abort();
        Task<string> DownloadSchedule(string group, bool isSession);
        Task<string> DownloadGroupListAsync();
    }
}