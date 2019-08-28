namespace MosPolytechHelper.Common.Interfaces
{
    using System.Threading.Tasks;

    interface IDownloader
    {
        Task<string> DownloadTimetable(string group);

        Task<string> DownloadGroupListAsync();
    }
}