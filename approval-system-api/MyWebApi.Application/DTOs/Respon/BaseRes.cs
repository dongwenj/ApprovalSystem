namespace MyWebApi.Application.DTOs.Respon
{
    public class BaseRes
    {
        public bool IsSuccess { get; set; } = true;
        public int Status { get; set; } = 200;
        public string Message { get; set; } = string.Empty;
    }
}
