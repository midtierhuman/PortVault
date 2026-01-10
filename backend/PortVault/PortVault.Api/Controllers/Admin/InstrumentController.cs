using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Models.Entities;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;

namespace PortVault.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
                var response = ApiResponse<IEnumerable<Instrument>>.SuccessResponse(results, "Instruments found");
                return Ok(response);
            }
            
            var instruments = await _repo.GetAllAsync();
            var allResponse = ApiResponse<IEnumerable<Instrument>>.SuccessResponse(instruments, "All instruments retrieved successfully");
            return Ok(allResponse);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var response = ApiResponse<Instrument>.SuccessResponse(instrument, "Instrument retrieved successfully");
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Instrument instrument)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid instrument data"));
            
            var created = await _repo.CreateAsync(instrument);
            var response = ApiResponse<Instrument>.SuccessResponse(created, "Instrument created successfully");
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] Instrument instrument)
        {
            if (id != instrument.Id && instrument.Id != 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("ID mismatch"));

            var updated = await _repo.UpdateAsync(id, instrument);
            if (updated == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var response = ApiResponse<Instrument>.SuccessResponse(updated, "Instrument updated successfully");
            return Ok(response);
        }

        [HttpPost("{id:long}/identifiers")]
        public async Task<IActionResult> AddIdentifier(long id, [FromBody] InstrumentIdentifier identifier)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));

            var created = await _repo.AddIdentifierAsync(id, identifier);
            var response = ApiResponse<InstrumentIdentifier>.SuccessResponse(created, "Identifier added successfully");
            return Ok(response);
        }

        [HttpPatch("{id:long}/identifiers/{identifierId:long}/move")]
        public async Task<IActionResult> MoveIdentifier(long id, long identifierId)
        {
            var result = await _repo.MoveIdentifierAsync(id, identifierId);
            if (result == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument or Identifier not found"));
            
            var response = ApiResponse<InstrumentIdentifier>.SuccessResponse(result, "Identifier moved successfully");
            return Ok(response);
        }

        [HttpDelete("identifiers/{identifierId:long}")]
        public async Task<IActionResult> DeleteIdentifier(long identifierId)
        {
            await _repo.DeleteIdentifierAsync(identifierId);
            var response = ApiResponse<object>.SuccessResponse(null, "Identifier deleted successfully");
            return Ok(response);
        }
    }
}
