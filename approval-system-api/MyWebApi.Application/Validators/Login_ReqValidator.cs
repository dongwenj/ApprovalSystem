namespace MyWebApi.Application.Validator
{
    public class Login_ReqValidator
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int Level { get; set; }
        public string Dept { get; set; }
    }
}
