namespace MosPolytechHelper.Common.Interfaces
{
    using MosPolytechHelper.Domain;
    using System.Threading.Tasks;

    interface ITimetableConverter
    {
        Task<FullTimetable> ConvertToFullTimetableAsync(string serializedObj);
        Task<string[]> ConvertToGroupList(string serializedObj);
    }
}