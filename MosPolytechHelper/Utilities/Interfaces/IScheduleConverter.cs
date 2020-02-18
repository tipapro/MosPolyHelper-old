namespace MosPolyHelper.Utilities.Interfaces
{
    using MosPolyHelper.Domains.ScheduleDomain;
    using System;
    using System.Threading.Tasks;

    public interface IScheduleConverter
    {
        Task<Schedule> ConvertToScheduleAsync(string serializedObj, Action<string> sendMessage);
        Task<string[]> ConvertToGroupList(string serializedObj);
    }
}