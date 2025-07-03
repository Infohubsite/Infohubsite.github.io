namespace Frontend.Models
{
    public class MonthlyUsage
    {
        public string Month { get; set; } = string.Empty;
        public long RuUsage { get; set; }
        public decimal StorageGibAvg { get; set; }
    }
}
