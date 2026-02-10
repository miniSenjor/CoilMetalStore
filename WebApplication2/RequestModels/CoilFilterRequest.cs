namespace WebApplication2.RequestModels
{
    public class CoilFilterRequest
    {
        public int? MinId { get; set; }
        public int? MaxId { get; set; }

        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }

        public DateTime? MinDateAdd { get; set; }
        public DateTime? MaxDateAdd { get; set; }

        public DateTime? MinDateDelete { get; set; }
        public DateTime? MaxDateDelete { get; set; }
    }
}
