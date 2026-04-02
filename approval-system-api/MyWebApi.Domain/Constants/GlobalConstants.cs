namespace MyWebApi.Domain.Constants
{
    public static class ErrorMessages
    {
        public const string NotEmpty = "{0}不可為空";
        public const string StringMustShorter = "{0}長度不可超過{1}字";
        public const string DateMustLongerThanToday = "{0}不可晚於今天";
        public const string NumberMustGreater = "{0}必須大於{1}的正整數";
    }

    public static class ExceptionMessages
    {
        public const string Status401Unauthorized = "權限不足";
        public const string Status409Conflict = "該筆資料已被修改過";
        public const string Status500InternalServerError = "系統發生錯誤";
    }

    public static class ConfigKeys
    {
        public const string DefaultConnection = "DefaultConnection";

        public static class Jwt
        {
            public const string Key = "Jwt:Key";
            public const string Issuer = "Jwt:Issuer";
            public const string Audience = "Jwt:Audience";
        }

        public static class MailSettings
        {
            public const string Host = "MailSettings:Host";
            public const string Port = "MailSettings:Port";
            public const string DisplayName = "MailSettings:DisplayName";
            public const string Mail = "MailSettings:Mail";
            public const string Password = "MailSettings:Password";
            public const string UserName = "MailSettings:UserName";
        }
    }

    public static class SystemIdentify
    {
        public const string TraceId = "TraceId";
        public const string UserId = "UserId";
        public const string Level = "Level";
        public const string Dept = "Dept";
        public const string Name = "Name";
        public const string Authorization = "Authorization";
        public const string Anonymous = "Anonymous";
        public const string UnknownUser = "UnknownUser";
        public const string Jwt = "Jwt";
        public const string Bearer = "Bearer";
    }
}
