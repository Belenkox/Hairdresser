using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hairdresser.Models
{
    public class OrderDetailed
    {
        public int ID { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public Employee Employee { get; set; }
    }
}
