namespace MosPolyHelper.Utilities
{
    using Android.Content;
    using MosPolyHelper.Utilities.Interfaces;
    using System;

    static class StringProvider
    {
        static ILogger logger;

        public static void SetUpLogger(ILoggerFactory loggerFactory)
        {
            if (logger == null)
            {
                logger = loggerFactory.Create(typeof(StringProvider).FullName);
            }
        }

        public static Context Context { get; set; }

        public static string GetString(StringId stringId)
        {
            if (Context == null)
            {
                return string.Empty;
            }
            try
            {
                switch (stringId)
                {
                    case StringId.ScheduleWasntFounded:
                        return Context.GetString(Resource.String.schedule_not_found);
                    case StringId.OfflineScheduleWasntFounded:
                        return Context.GetString(Resource.String.offline_schedule_not_found);
                    case StringId.OfflineScheduleWasFounded:
                        return Context.GetString(Resource.String.offline_schedule_found);
                    case StringId.GroupListWasntFounded:
                        return Context.GetString(Resource.String.group_list_not_found);
                    case StringId.OfflineGroupListWasntFounded:
                        return Context.GetString(Resource.String.offline_group_list_not_found);
                    case StringId.OfflineGroupListWasFounded:
                        return Context.GetString(Resource.String.offline_group_list_found);
                }
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "StringProviderFail {stringId}", stringId);
            }
            return string.Empty;
        }
    }

    public enum StringId
    {
        ScheduleWasntFounded,
        OfflineScheduleWasntFounded,
        OfflineScheduleWasFounded,
        GroupListWasntFounded,
        OfflineGroupListWasntFounded,
        OfflineGroupListWasFounded,
        Buildings
    }
}