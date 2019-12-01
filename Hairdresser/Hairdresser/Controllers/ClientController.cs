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
using System.Security.Claims;
using System.Threading;

namespace Hairdresser.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : Controller
    {
        private readonly HairdresserContext _contextDb;

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

            if (identity.FindFirst(ClaimTypes.Role).Value == "Administrator"
                ||int.Parse(sid) == _contextDb.Clients.Where(j => j.ID == id).FirstOrDefault().fk_User) //gali peržiūrėti tik administratorius arba pats klientas
            {
                var client = await _contextDb.Clients.Where(j => j.ID == id).FirstOrDefaultAsync();

                if (client == null)
                {
                    return NotFound();
                }

                return client;
            }

            return ValidationProblem();

        }

        //PUT: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Client client)
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
            Client klientukas = await _contextDb.Clients.Where(j => j.ID == id).FirstOrDefaultAsync();
            
            if (klientukas == null)
            {
                return BadRequest("Couldnt find matching ID of client");
            }
            if (identity.FindFirst(ClaimTypes.Role).Value == "Administrator"
                ||int.Parse(sid) == klientukas.fk_User) // gali redaguoti tik administratorius ir pats klientas
            {
                if (klientukas != null)
                {
                    _contextDb.Entry(klientukas).State = EntityState.Detached;
                }

                client.ID = id;
                client.fk_User = klientukas.fk_User;

                _contextDb.Entry(client).State = EntityState.Modified;
                await _contextDb.SaveChangesAsync();

                return Ok(client);
            }
            else
            {
                return ValidationProblem();
            }

        }

        //DELETE: api/Client/4
        [Authorize(Roles = "Administrator,Client")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
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
            if (identity.FindFirst(ClaimTypes.Role).Value == "Administrator"
                || int.Parse(sid) == _contextDb.Clients.Where(j => j.ID == id).FirstOrDefault().fk_User)
            {
                var client = await _contextDb.Clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                _contextDb.Clients.Remove(client);
                await _contextDb.SaveChangesAsync();

                return Ok(client);
            }
            else
            {
                return ValidationProblem();
            }
        }

        //POST: api/Client
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            if (_contextDb.Clients.Where(x => x.Mail == client.Mail).FirstOrDefault() != null) //patikrina ar nera jau tokio email
            {
                return ValidationProblem();
            }
            _contextDb.Clients.Add(client);
            await _contextDb.SaveChangesAsync();

            return Ok(client);
        }
    }
}
