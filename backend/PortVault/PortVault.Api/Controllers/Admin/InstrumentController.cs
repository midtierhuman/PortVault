using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Repositories;

namespace PortVault.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InstrumentController : ControllerBase
    {
        private readonly IInstrumentRepository _repo;

        public InstrumentController(IInstrumentRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var results = await _repo.SearchAsync(search);
                return Ok(results);
            }
            
            var instruments = await _repo.GetAllAsync();
            return Ok(instruments);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) return NotFound();
            return Ok(instrument);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Instrument instrument)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var created = await _repo.CreateAsync(instrument);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] Instrument instrument)
        {
            if (id != instrument.Id && instrument.Id != 0)
                return BadRequest("ID mismatch");

            var updated = await _repo.UpdateAsync(id, instrument);
            if (updated == null) return NotFound();
            
            return Ok(updated);
        }

        [HttpPost("{id:long}/identifiers")]
        public async Task<IActionResult> AddIdentifier(long id, [FromBody] InstrumentIdentifier identifier)
        {
            // Verify instrument exists
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) return NotFound("Instrument not found");

            var created = await _repo.AddIdentifierAsync(id, identifier);
            return Ok(created);
        }

        [HttpDelete("identifiers/{identifierId:long}")]
        public async Task<IActionResult> DeleteIdentifier(long identifierId)
        {
            await _repo.DeleteIdentifierAsync(identifierId);
            return Ok(new { message = "Identifier deleted" });
        }
    }
}
