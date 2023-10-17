using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Customer : BaseEntity
    {
        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }     

        public int DiscountValue { get; set; }

        public Person Person { get; set; }
        public ICollection<Receipt> Receipts { get; set; }
    }
}
