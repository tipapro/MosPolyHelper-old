namespace MosPolyHelper.Common.Interfaces
{
    using MosPolyHelper.Domain;
    using System.Threading.Tasks;

    public interface IScheduleConverter
    {
        Task<Schedule> ConvertToScheduleAsync(string serializedObj);
        Task<string[]> ConvertToGroupList(string serializedObj);
    }
}