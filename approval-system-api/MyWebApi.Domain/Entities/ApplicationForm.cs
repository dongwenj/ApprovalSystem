using System.ComponentModel.DataAnnotations;

namespace MyWebApi.Domain.Entities
{
    public class ApplicationForm
    {
        public int ApplicationNo { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int ApplicantId { get; set; }
        public string Reason { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? SignerId { get; set; }
        public int? SubmitterId { get; set; }
        public DateTime UpdateDate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
