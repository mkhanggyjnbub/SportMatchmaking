namespace Services.Helpers
{
    public static class StatusMapper
    {
        public static string JoinRequestStatus(int status)
        {
            return status switch
            {
                1 => "Pending",
                2 => "Accepted",
                3 => "Rejected",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }

        public static string MatchPostStatus(int status)
        {
            return status switch
            {
                1 => "Open",
                2 => "Full",
                3 => "Confirmed",
                4 => "Completed",
                5 => "Cancelled",
                6 => "Expired",
                _ => "Unknown"
            };
        }
    }
}