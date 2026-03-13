namespace MyWebApi.Application.DTOs.Request
{
    public class ApplicationFormAdd_Req
    {
        public DateTime ApplicationDate { get; set; }
        public string Reason { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public class Item
        {
            public string ItemName { get; set; }
            public string Unit { get; set; }
            public int Quantity { get; set; }
            public int Price { get; set; }
        }
    }
}

