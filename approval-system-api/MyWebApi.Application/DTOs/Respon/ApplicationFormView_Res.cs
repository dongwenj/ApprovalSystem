namespace MyWebApi.Application.DTOs.Respon
{
    public class ApplicationFormView_Res : BaseRes
    {
        public int ApplicationNo { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string UserName { get; set; }
        public string DeptId { get; set; }
        public string Reason { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public class Item
        {
            public int Id { get; set; }
            public string ItemName { get; set; }
            public string Unit { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }
        }
    }
}
