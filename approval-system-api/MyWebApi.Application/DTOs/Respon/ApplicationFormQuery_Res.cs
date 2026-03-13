namespace MyWebApi.Application.DTOs.Respon
{
    public class ApplicationFormQuery_Res : BaseRes
    {
        public string ApplicationNo { get; set; }
        public string ApplicationDate { get; set; }
        public string UserName { get; set; }
        public string DeptId { get; set; }
        public string Reason { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
