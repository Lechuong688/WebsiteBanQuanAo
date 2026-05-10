using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entity
{
    public class ChatSessionEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string? Title { get; set; }

        public int? ProductId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsAdminSupport { get; set; }
        public virtual ICollection<ChatMessageEntity>ChatMessages{ get; set; } = new List<ChatMessageEntity>();

    }
}
