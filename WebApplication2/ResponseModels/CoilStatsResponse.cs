namespace WebApplication2.ResponseModels
{
    public class CoilStatsResponse
    {
        public int CountAdd { get; set; } = 0;
        public int CountDelete { get; set; } = 0;

        public double AvLength { get; set; } = 0;
        public double MaxLength { get; set; } = 0;
        public double MinLength { get; set; } = 0;
        public double AvWeight { get; set; } = 0;
        public double MaxWeight { get; set; } = 0;
        public double MinWeight { get; set; } = 0;
        public double SumWeight { get; set; } = 0;
        public TimeSpan? MinTimeBeforeDelete { get; set; }
        public TimeSpan? MaxTimeBeforeDelete { get; set; }

        public DailyStats DailyCoilsStats { get; set; }
        public DailyStats DailyWeightStats { get; set; }

        public class DailyStats
        {
            public DateTime Date { get; set; }
            public double MaxValue { get; set; }
            public DateTime DateMin { get; set; }
            public double MinValue { get; set; }
        }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

}
