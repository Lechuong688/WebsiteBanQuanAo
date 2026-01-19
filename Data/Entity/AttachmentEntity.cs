using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class AttachmentEntity
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsDeleted { get; set; }
        public int EntityId { get; set; }
        public string? EntityType {  get; set; }
    }
}
