namespace NabdSchool.Web.ViewModels
{
    public class ArchiveStatisticsViewModel
    {
        public int TotalLogs { get; set; }
        public List<ActionStatistic> ActionsByType { get; set; }
        public List<TableStatistic> ActionsByTable { get; set; }
        public List<UserActivityStatistic> TopUsers { get; set; }
        public List<DateActivityStatistic> ActivityByDate { get; set; }
    }

    public class ActionStatistic
    {
        public string ActionType { get; set; }
        public int Count { get; set; }
    }

    public class TableStatistic
    {
        public string TableName { get; set; }
        public int Count { get; set; }
    }

    public class UserActivityStatistic
    {
        public string UserName { get; set; }
        public int ActionCount { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class DateActivityStatistic
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
