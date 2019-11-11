using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hairdresser.Models;
using Hairdresser.Controllers;
using System;
using Hairdresser.Data;
using Microsoft.AspNetCore.Authorization;

namespace Hairdresser.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : Controller
    {
        private readonly HairdresserContext _contextDb;

        public List<Client> clients = new List<Client>();
    
        public ClientController(HairdresserContext contextDb)
        {
            _contextDb = contextDb;
        }

        // GET: api/Client
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _contextDb.Clients.ToListAsync();
        }

        // GET: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpGet("{id}")]
            public async Task<ActionResult<Client>> GetClient(int id)
            {
                var client = await _contextDb.Clients.Where(j => j.ID == id).FirstOrDefaultAsync();

                if (client == null)
                {
                    return NotFound();
                }

                return client;
            }

        //PUT: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpPut("{id}")]
            public async Task<IActionResult> PutClient(int id, Client client)
            {
                if (id != client.ID)
                {
                    return BadRequest();
                }

                _contextDb.Entry(client).State = EntityState.Modified;
                await _contextDb.SaveChangesAsync();

                return NoContent();
            }

        //DELETE: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteClient(int id)
            {
                var client = await _contextDb.Clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                _contextDb.Clients.Remove(client);
                await _contextDb.SaveChangesAsync();

                return NoContent();
            }

        //POST: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _contextDb.Clients.Add(client);
            await _contextDb.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.ID }, client);
        }
    }
}
