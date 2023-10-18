using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public int DiscountValue { get; set; }
        public ICollection<int> ReceiptsIds { get; set; }
    }
}
