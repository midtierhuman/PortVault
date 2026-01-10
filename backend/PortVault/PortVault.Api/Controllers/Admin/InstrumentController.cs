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
            IEnumerable<Instrument> instruments;
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                instruments = await _repo.SearchAsync(search);
            }
            else
            {
                instruments = await _repo.GetAllAsync();
            }
            
            var response = instruments.Select(i => new InstrumentResponse
            {
                Id = i.Id,
                Type = i.Type.ToString(),
                Name = i.Name,
                Identifiers = i.Identifiers.Select(id => new InstrumentIdentifierResponse
                {
                    Id = id.Id,
                    Type = id.Type.ToString(),
                    Value = id.Value,
                    ValidFrom = id.ValidFrom,
                    ValidTo = id.ValidTo
                }).ToList()
            });
            
            var message = !string.IsNullOrWhiteSpace(search) 
                ? "Instruments found" 
                : "All instruments retrieved successfully";
            
            var apiResponse = ApiResponse<IEnumerable<InstrumentResponse>>.SuccessResponse(response, message);
            return Ok(apiResponse);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var response = new InstrumentResponse
            {
                Id = instrument.Id,
                Type = instrument.Type.ToString(),
                Name = instrument.Name,
                Identifiers = instrument.Identifiers.Select(id => new InstrumentIdentifierResponse
                {
                    Id = id.Id,
                    Type = id.Type.ToString(),
                    Value = id.Value,
                    ValidFrom = id.ValidFrom,
                    ValidTo = id.ValidTo
                }).ToList()
            };
            
            var apiResponse = ApiResponse<InstrumentResponse>.SuccessResponse(response, "Instrument retrieved successfully");
            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInstrumentRequest request)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid instrument data"));
            
            if (!Enum.TryParse<InstrumentType>(request.Type, true, out var instrumentType))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid instrument type. Valid types are: MF, EQ"));
            
            var instrument = new Instrument
            {
                Type = instrumentType,
                Name = request.Name
            };
            
            var created = await _repo.CreateAsync(instrument);
            
            var response = new InstrumentResponse
            {
                Id = created.Id,
                Type = created.Type.ToString(),
                Name = created.Name,
                Identifiers = new List<InstrumentIdentifierResponse>()
            };
            
            var apiResponse = ApiResponse<InstrumentResponse>.SuccessResponse(response, "Instrument created successfully");
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, apiResponse);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateInstrumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid instrument data"));
            
            if (!Enum.TryParse<InstrumentType>(request.Type, true, out var instrumentType))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid instrument type. Valid types are: MF, EQ"));
            
            var instrument = new Instrument
            {
                Id = id,
                Type = instrumentType,
                Name = request.Name
            };
            
            var updated = await _repo.UpdateAsync(id, instrument);
            if (updated == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var response = new InstrumentResponse
            {
                Id = updated.Id,
                Type = updated.Type.ToString(),
                Name = updated.Name,
                Identifiers = updated.Identifiers?.Select(id => new InstrumentIdentifierResponse
                {
                    Id = id.Id,
                    Type = id.Type.ToString(),
                    Value = id.Value,
                    ValidFrom = id.ValidFrom,
                    ValidTo = id.ValidTo
                }).ToList() ?? new List<InstrumentIdentifierResponse>()
            };
            
            var apiResponse = ApiResponse<InstrumentResponse>.SuccessResponse(response, "Instrument updated successfully");
            return Ok(apiResponse);
        }

        [HttpPost("{id:long}/identifiers")]
        public async Task<IActionResult> AddIdentifier(long id, [FromBody] AddInstrumentIdentifierRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid identifier data"));
            
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            if (!Enum.TryParse<IdentifierType>(request.Type, true, out var identifierType))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid identifier type. Valid types are: ISIN, TICKER, NSE_SYMBOL, BSE_CODE, SCHEME_CODE"));
            
            var identifier = new InstrumentIdentifier
            {
                Type = identifierType,
                Value = request.Value,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo
            };
            
            var created = await _repo.AddIdentifierAsync(id, identifier);
            
            var response = new InstrumentIdentifierResponse
            {
                Id = created.Id,
                Type = created.Type.ToString(),
                Value = created.Value,
                ValidFrom = created.ValidFrom,
                ValidTo = created.ValidTo
            };
            
            var apiResponse = ApiResponse<InstrumentIdentifierResponse>.SuccessResponse(response, "Identifier added successfully");
            return Ok(apiResponse);
        }

        [HttpPatch("{id:long}/identifiers/{identifierId:long}/move")]
        public async Task<IActionResult> MoveIdentifier(long id, long identifierId)
        {
            var result = await _repo.MoveIdentifierAsync(id, identifierId);
            if (result == null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument or Identifier not found"));
            
            var response = new InstrumentIdentifierResponse
            {
                Id = result.Id,
                Type = result.Type.ToString(),
                Value = result.Value,
                ValidFrom = result.ValidFrom,
                ValidTo = result.ValidTo
            };
            
            var apiResponse = ApiResponse<InstrumentIdentifierResponse>.SuccessResponse(response, "Identifier moved successfully");
            return Ok(apiResponse);
        }

        [HttpPost("{sourceId:long}/migrate")]
        public async Task<IActionResult> MigrateInstrument(long sourceId, [FromBody] MigrateInstrumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid migration request"));
            
            var sourceInstrument = await _repo.GetByIdAsync(sourceId);
            if (sourceInstrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Source instrument not found"));
            
            var targetInstrument = await _repo.GetByIdAsync(request.TargetInstrumentId);
            if (targetInstrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Target instrument not found"));
            
            if (sourceId == request.TargetInstrumentId)
                return BadRequest(ApiResponse<object>.ErrorResponse("Source and target instruments cannot be the same"));
            
            try
            {
                var (identifiersMoved, transactionsMigrated, holdingsMigrated) = 
                    await _repo.MigrateInstrumentAsync(sourceId, request.TargetInstrumentId);
                
                var migrationResponse = new InstrumentMigrationResponse
                {
                    SourceInstrumentId = sourceId,
                    SourceInstrumentName = sourceInstrument.Name,
                    TargetInstrumentId = request.TargetInstrumentId,
                    TargetInstrumentName = targetInstrument.Name,
                    IdentifiersMoved = identifiersMoved,
                    TransactionsMigrated = transactionsMigrated,
                    HoldingsMigrated = holdingsMigrated,
                    Message = $"Successfully migrated {identifiersMoved} identifier(s), {transactionsMigrated} transaction(s), and {holdingsMigrated} holding(s) from '{sourceInstrument.Name}' to '{targetInstrument.Name}'"
                };
                
                var response = ApiResponse<InstrumentMigrationResponse>.SuccessResponse(
                    migrationResponse, 
                    "Instrument migration completed successfully"
                );
                
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error during migration: {ex.Message}"));
            }
        }

        [HttpGet("{id:long}/dependencies")]
        public async Task<IActionResult> GetDependencies(long id)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var (transactionCount, holdingCount, identifierCount) = await _repo.GetInstrumentDependenciesAsync(id);
            
            var canDelete = transactionCount == 0 && holdingCount == 0;
            
            var message = canDelete
                ? identifierCount > 0
                    ? "Instrument can be deleted. All identifiers will be removed."
                    : "Instrument can be deleted."
                : $"Cannot delete instrument. It is referenced by {transactionCount} transaction(s) and {holdingCount} holding(s). Please reassign these before deletion.";
            
            var response = new InstrumentDependenciesResponse
            {
                InstrumentId = instrument.Id,
                InstrumentName = instrument.Name,
                CanDelete = canDelete,
                TransactionCount = transactionCount,
                HoldingCount = holdingCount,
                IdentifierCount = identifierCount,
                Identifiers = instrument.Identifiers.Select(i => new InstrumentIdentifierResponse
                {
                    Id = i.Id,
                    Type = i.Type.ToString(),
                    Value = i.Value,
                    ValidFrom = i.ValidFrom,
                    ValidTo = i.ValidTo
                }).ToList(),
                Message = message
            };
            
            var apiResponse = ApiResponse<InstrumentDependenciesResponse>.SuccessResponse(response, "Dependency check completed");
            return Ok(apiResponse);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteInstrument(long id)
        {
            var instrument = await _repo.GetByIdAsync(id);
            if (instrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var (transactionCount, holdingCount, identifierCount) = await _repo.GetInstrumentDependenciesAsync(id);
            
            if (transactionCount > 0 || holdingCount > 0)
            {
                var dependencyResponse = new InstrumentDependenciesResponse
                {
                    InstrumentId = instrument.Id,
                    InstrumentName = instrument.Name,
                    CanDelete = false,
                    TransactionCount = transactionCount,
                    HoldingCount = holdingCount,
                    IdentifierCount = identifierCount,
                    Identifiers = instrument.Identifiers.Select(i => new InstrumentIdentifierResponse
                    {
                        Id = i.Id,
                        Type = i.Type.ToString(),
                        Value = i.Value,
                        ValidFrom = i.ValidFrom,
                        ValidTo = i.ValidTo
                    }).ToList(),
                    Message = $"Cannot delete instrument. It is referenced by {transactionCount} transaction(s) and {holdingCount} holding(s). Please move all identifiers to another instrument first."
                };
                
                var errorResponse = new ApiResponse<InstrumentDependenciesResponse>
                {
                    Success = false,
                    Message = "Cannot delete instrument due to existing dependencies",
                    Data = dependencyResponse
                };
                
                return Conflict(errorResponse);
            }
            
            try
            {
                await _repo.DeleteInstrumentAsync(id);
                var response = ApiResponse<object>.SuccessResponse(null, $"Instrument '{instrument.Name}' and its {identifierCount} identifier(s) deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error deleting instrument: {ex.Message}"));
            }
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
