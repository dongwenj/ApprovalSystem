namespace MyWebApi.Application.DTOs.Request
{
    public class ApplicationFormEdit_Req
    {
        public int ApplicationNo { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Reason { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public byte[] RowVersion { get; set; }

        public class Item
        {
            public int Id { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public int Price { get; set; }
            public string Unit { get; set; }
        }
    }
}

