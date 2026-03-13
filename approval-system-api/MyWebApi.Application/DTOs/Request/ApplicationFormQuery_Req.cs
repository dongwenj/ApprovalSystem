using System.Text.Json.Serialization;

namespace MyWebApi.Application.DTOs.Request
{
    public class ApplicationFormQuery_Req
    {
        public string? Reason {  get; set; }
        public string? ApplicationDate {  get; set; }
        public string? DeptId {  get; set; }
        [JsonIgnore]
        public UserDataBase User { get; set; } = new UserDataBase();
        public int PageNumber { get; set; } = 1; // 預設第1頁
        public int PageSize { get; set; } = 10;  // 預設10筆

    }
}
