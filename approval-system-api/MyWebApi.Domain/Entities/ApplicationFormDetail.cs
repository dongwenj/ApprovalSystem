namespace MyWebApi.Domain.Entities
{
    public class ApplicationFormDetail
    {
        public int Id { get; set; }
        public int AppFormNo { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
