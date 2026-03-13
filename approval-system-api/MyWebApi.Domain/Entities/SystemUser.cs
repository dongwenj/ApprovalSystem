namespace MyWebApi.Domain.Entities
{
    public class SystemUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Level { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? SupervisorId { get; set; }
        public string DeptId { get; set; }
    }
}
