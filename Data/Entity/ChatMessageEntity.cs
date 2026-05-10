using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace Data.Entity
{
    public class ChatMessageEntity
    {
        public int Id { get; set; }

        public int ChatSessionId { get; set; }

        public string SenderType { get; set; }

        public string Message { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }
        [ForeignKey("ChatSessionId")] public virtual ChatSessionEntity ChatSession { get; set; }
    }
}
