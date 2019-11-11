using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hairdresser.Models
{
    public class Order
    {
        public int ID { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public int fk_Client { get; set; }
        public int fk_Employee { get; set; }
    }
}
