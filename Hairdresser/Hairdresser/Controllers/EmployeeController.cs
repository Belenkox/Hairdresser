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
    public class EmployeeController : ControllerBase
    {
        private readonly HairdresserContext _contextDb;

        public EmployeeController(HairdresserContext contextDb)
        {
            _contextDb = contextDb;
        }

        // GET: api/Employee
        [Authorize(Roles = "Administrator,Employee,Client")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _contextDb.Employees.ToListAsync();
        }

        // GET: api/Employee/4
        [Authorize(Roles = "Administrator,Employee,Client")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _contextDb.Employees.Where(j => j.ID == id).FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        //POST: api/Employee
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _contextDb.Employees.Add(employee);
            await _contextDb.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.ID }, employee);
        }

        //PUT: api/Employee/4
        [Authorize(Roles = "Administrator,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.ID)
            {
                return BadRequest();
            }

            _contextDb.Entry(employee).State = EntityState.Modified;
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }

        //DELETE: api/Employee/4
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _contextDb.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            _contextDb.Employees.Remove(employee);
            await _contextDb.SaveChangesAsync();

            return NoContent();
        }
    }
}