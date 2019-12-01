using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hairdresser.Models;

namespace Hairdresser.Models
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int fk_User { get; set; }
    }
}
