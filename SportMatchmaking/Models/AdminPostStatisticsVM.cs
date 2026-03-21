namespace SportMatchmaking.Models
{
    public class AdminPostStatisticsVM
    {
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();

        public List<string> SportLabels { get; set; } = new();
        public List<int> SportCounts { get; set; } = new();

        public List<string> OpenOrFullSportLabels { get; set; } = new();
        public List<int> OpenOrFullSportCounts { get; set; } = new();

        public List<int> WeekLabels { get; set; } = new();
        public List<int> WeeklyPostCounts { get; set; } = new();

        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();

        public int TotalSports { get; set; }
        public int TotalPosts { get; set; }
    }
}
