namespace MosPolyHelper.Common.Interfaces
{
    using System.Threading.Tasks;

    public interface IScheduleDownloader
    {
        Task<string> DownloadSchedule(string group, bool isSession);
        Task<string> DownloadGroupListAsync();
    }
}