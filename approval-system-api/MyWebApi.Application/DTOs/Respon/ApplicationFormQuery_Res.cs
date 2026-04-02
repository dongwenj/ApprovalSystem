namespace MyWebApi.Application.DTOs.Respon
{
    public class ApplicationFormQuery_Res : BaseRes
    {
        public List<DataItem> DataList { get; set; } = new List<DataItem>();

        public class DataItem
        {
            public string ApplicationNo { get; set; }
            public string ApplicationDate { get; set; }
            public int Status { get; set; }
            public string UserName { get; set; }
            public string SignerName { get; set; }
            public string DeptId { get; set; }
            public string Reason { get; set; }
            public byte[] RowVersion { get; set; }
            public int TotalCount { get; set; }
        }

        public int TotalCount { get; set; }
    }
}
