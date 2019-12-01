using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hairdresser.Models;
using System;
using Hairdresser.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hairdresser.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly HairdresserContext _contextDb;

        public OrderController(HairdresserContext contextDb)
        {
            _contextDb = contextDb;            
        }

        // GET: api/Order
        [Authorize(Roles = "Administrator,Employee,Client")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailed>>> GetOrders() //gražina visus su vartotoju susijusius užsakymus
        {
            List<Order> orders = await _contextDb.Orders.ToListAsync();
            List<OrderDetailed> ordersD = new List<OrderDetailed>();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            switch (identity.FindFirst(ClaimTypes.Role).Value)
            {
                case "Administrator" :
                    foreach (Order order in orders)
                    {
                        ordersD.Add(new OrderDetailed
                        {
                            ID = order.ID,
                            ServiceName = order.ServiceName,
                            Date = order.Date,
                            Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                            Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                        });
                    }
                    break;
                case "Employee" :
                    Employee employee = await _contextDb.Employees.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefaultAsync();
                    foreach (Order order in orders)
                    {
                        if (employee.ID == order.fk_Employee)
                        {
                            ordersD.Add(new OrderDetailed
                            {
                                ID = order.ID,
                                ServiceName = order.ServiceName,
                                Date = order.Date,
                                Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                                Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                            });
                        }
                    }
                    break;
                case "Client":
                    Client client = await _contextDb.Clients.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefaultAsync();
                    foreach (Order order in orders)
                    {
                        if (client.ID == order.fk_Client)
                        {
                            ordersD.Add(new OrderDetailed
                            {
                                ID = order.ID,
                                ServiceName = order.ServiceName,
                                Date = order.Date,
                                Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                                Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                            });
                        }
                    }
                    break;
                default:
                    break;
            }

            return ordersD;
        }

        // GET: api/Order/4
        [Authorize(Roles = "Administrator,Employee,Client")]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailed>> GetOrder(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            OrderDetailed orderD = new OrderDetailed();

            switch (identity.FindFirst(ClaimTypes.Role).Value)
            {
                case "Administrator":
                    orderD = new OrderDetailed
                    {
                        ID = order.ID,
                        ServiceName = order.ServiceName,
                        Date = order.Date,
                        Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                        Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                    };
                    break;
                case "Employee":
                    Employee employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync();
                    if (int.Parse(sid) == employee.fk_User)
                    {
                        orderD = new OrderDetailed
                        {
                            ID = order.ID,
                            ServiceName = order.ServiceName,
                            Date = order.Date,
                            Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                            Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                        };
                    }
                    else
                    {
                        return ValidationProblem();
                    }
                    break;
                case "Client":
                    Client client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync();
                    if (int.Parse(sid) == client.fk_User)
                    {
                        orderD = new OrderDetailed
                        {
                            ID = order.ID,
                            ServiceName = order.ServiceName,
                            Date = order.Date,
                            Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                            Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                        };
                    }
                    else
                    {
                        return ValidationProblem();
                    }
                    break;
                default:
                    break;
            }
            
            return orderD;
        }

        //POST: api/Order
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostOrder(Order order) //be fk_client ir ID
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            Client clientFromClaim = await _contextDb.Clients.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefaultAsync();

            if (clientFromClaim == null)//patikrina ar klientas egzistuoja
            {
                return ValidationProblem();
            }
            else
            {
                order.fk_Client = clientFromClaim.ID;
            }

            Employee employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync();

            if (employee == null)//patikrina ar darbuotojas gali buti priskirtas uzsakymui
            {
                return ValidationProblem();
            }

            _contextDb.Orders.Add(order);
            await _contextDb.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.ID }, order);
        }

        //PUT: api/Order/4
        [Authorize(Roles = "Client")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order) //be fk_client ir ID
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            Client clientFromClaim = await _contextDb.Clients.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefaultAsync();

            if (clientFromClaim == null)//užtikrina, kad klientas nėra null
            {
                return ValidationProblem();
            }
            else
            {
                order.fk_Client = clientFromClaim.ID;
            }

            Employee employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync();

            if (employee == null)//patikrina ar darbuotojas gali buti priskirtas uzsakymui
            {
                return ValidationProblem();
            }
            order.ID = id;

            _contextDb.Entry(order).State = EntityState.Modified;
            await _contextDb.SaveChangesAsync();

            return Ok(order);
        }

        //DELETE: api/Order/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (identity.FindFirst(ClaimTypes.Role).Value == "Administrator" ||
                int.Parse(sid) == _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefault().fk_User)
            {
                

                _contextDb.Orders.Remove(order);

                await _contextDb.SaveChangesAsync();

                return Ok(order);
            }

            return BadRequest("deletion failed");
        }

        //GET: api/Order/2/Client
        [Authorize(Roles = "Administrator,Employee")]
        [HttpGet("{id}/Client")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            Order order = await _contextDb.Orders.Where(j => j.ID == id).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.fk_Employee == _contextDb.Employees.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefault().ID)
            {
                Client client = await _contextDb.Clients.FindAsync(order.fk_Client);

                if (client == null)
                {
                    return NotFound();
                }

                return client;
            }

            return NotFound();
        }

        //GET: api/Order/2/Employee
        [Authorize(Roles = "Administrator,Client")]
        [HttpGet("{id}/Employee")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            string sid;
            if (identity != null)
            {
                sid = identity.FindFirst("id").Value;
            }
            else
            {
                return ValidationProblem();
            }

            Order order = await _contextDb.Orders.Where(j => j.ID == id).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.fk_Client == _contextDb.Clients.Where(j => j.fk_User == int.Parse(sid)).FirstOrDefault().ID)
            {
                Employee employee = await _contextDb.Employees.FindAsync(order.fk_Employee);

                if (employee == null)
                {
                    return NotFound();
                }

                return employee;
            }

            return NotFound();
        }
    }

}
