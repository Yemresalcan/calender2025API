namespace CalendarAPI.Models
{
    public class Week
    {
        public string? Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<int> Days { get; set; } = new List<int>();
        public string MonthId { get; set; } = string.Empty;
    }
} 