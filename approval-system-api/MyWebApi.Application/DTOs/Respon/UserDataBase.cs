using MyWebApi.Application.DTOs.Respon;

public class UserDataBase : BaseRes
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public int Level { get; set; }
    public string? Dept { get; set; }
}