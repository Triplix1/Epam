using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Receipt : BaseEntity
    {
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        
        public DateTime OperationDate { get; set; }

        public bool IsCheckedOut { get; set; }

        public Customer Customer { get; set; }
        public ICollection<ReceiptDetail> ReceiptDetails { get;}
    }
}
