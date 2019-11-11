using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hairdresser.Models;
using System;
using Hairdresser.Data;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailed>>> GetOrders()
        {
            List<Order> orders = await _contextDb.Orders.ToListAsync();
            List<OrderDetailed> ordersD = new List<OrderDetailed>();
            foreach(Order order in orders)
            {
                ordersD.Add(new OrderDetailed {
                    ID = order.ID,
                    ServiceName = order.ServiceName,
                    Date = order.Date,
                    Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                    Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
                });       
            }

            return ordersD;
        }

        // GET: api/Order/4
        [Authorize(Roles = "Administrator,Employee,Client")]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailed>> GetOrder(int id)
        {
            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            OrderDetailed orderD = new OrderDetailed
            {
                ID = order.ID,
                ServiceName = order.ServiceName,
                Date = order.Date,
                Client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(),
                Employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync()
            };
            
            return orderD;
        }

        //POST: api/Order
        [Authorize(Roles = "Administrator,Client")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostOrder(Order order)
        {
            Client client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync();

            if (client == null)//patikrina ar klientas gali buti priskirtas uzsakymui
            {
                return ValidationProblem();
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
        [Authorize(Roles = "Administrator,Client")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.ID)
            {
                return BadRequest();
            }
            Client client = await _contextDb.Clients.Where(j => j.ID == order.fk_Client).FirstOrDefaultAsync(); 

            if (client == null)//patikrina ar klientas gali buti priskirtas uzsakymui
            {
                return ValidationProblem();
            }

            Employee employee = await _contextDb.Employees.Where(j => j.ID == order.fk_Employee).FirstOrDefaultAsync();

            if (employee == null)//patikrina ar darbuotojas gali buti priskirtas uzsakymui
            {
                return ValidationProblem();
            }

            _contextDb.Entry(order).State = EntityState.Modified;
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }

        //DELETE: api/Order/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            _contextDb.Orders.Remove(order);
            
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }

        //GET: api/Order/2/Client
        [Authorize(Roles = "Administrator,Employee")]
        [HttpGet("{id}/Client")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            Client client = await _contextDb.Clients.FindAsync(order.fk_Client);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        //GET: api/Order/2/Employee
        [Authorize(Roles = "Administrator,Client")]
        [HttpGet("{id}/Employee")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var order = await _contextDb.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            Employee employees = await _contextDb.Employees.FindAsync(order.fk_Employee);

            if (employees == null)
            {
                return NotFound();
            }

            return employees;
        }

        /*//PUT: api/Order/2/Client
        [HttpPut("{id}/Client")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            _contextDb.Entry(client).State = EntityState.Modified;
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }

        //PUT: api/Order/2/Employee
        [HttpPut("{id}/Employee")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            _contextDb.Entry(employee).State = EntityState.Modified;
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }

        //POST: api/Order/2/Client
        [HttpPost("{id}/Client")]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _contextDb.Clients.Add(client);
            await _contextDb.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.ID }, client);
        }*/

        //POST: api/Order/2/Employee

        //DELETE: api/Order/2/Client
        //DELETE: api/Order/2/Employee
    }

}
