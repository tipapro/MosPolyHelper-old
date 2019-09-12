namespace MosPolytechHelper.Common.Interfaces
{
    using System.Threading.Tasks;

    public interface IDownloader
    {
        Task<string> DownloadSchedule(string group);

        Task<string> DownloadGroupListAsync();
    }
}