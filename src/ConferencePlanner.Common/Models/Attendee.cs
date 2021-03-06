using System.ComponentModel.DataAnnotations;

namespace ConferencePlanner.Models
{
    public class Attendee
    {
        public int ID { get; set; }

        public string DirectoryObjectId { get; set; }

        [StringLength(200)]
        public virtual string FirstName { get; set; }

        [StringLength(200)]
        public virtual string LastName { get; set; }

        [Required]
        [StringLength(200)]
        public virtual string UserName { get; set; }

        [StringLength(256)]
        public virtual string EmailAddress { get; set; }
    }
}
