using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebApi.Application.DTOs.Respon
{
    public class Login_Res
    {
        public string? UserName { get; set; }
        public int Level { get; set; }
        public string? Dept { get; set; }
        public string Token { get; set; }
    }
}
