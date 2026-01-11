using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Models.Entities;
using PortVault.Api.Repositories;

namespace PortVault.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CorporateActionController : ControllerBase
    {
        private readonly ICorporateActionRepository _repo;
        private readonly IInstrumentRepository _instrumentRepo;

        public CorporateActionController(ICorporateActionRepository repo, IInstrumentRepository instrumentRepo)
        {
            _repo = repo;
            _instrumentRepo = instrumentRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var corporateActions = await _repo.GetAllAsync();
            
            var response = corporateActions.Select(ca => new CorporateActionResponse
            {
                Id = ca.Id,
                Type = ca.Type.ToString(),
                ExDate = ca.ExDate,
                ParentInstrumentId = ca.ParentInstrumentId,
                ParentInstrumentName = ca.ParentInstrument?.Name ?? string.Empty,
                ChildInstrumentId = ca.ChildInstrumentId,
                ChildInstrumentName = ca.ChildInstrument?.Name,
                RatioNumerator = ca.RatioNumerator,
                RatioDenominator = ca.RatioDenominator,
                CostPercentageAllocated = ca.CostPercentageAllocated
            });
            
            var apiResponse = ApiResponse<IEnumerable<CorporateActionResponse>>.SuccessResponse(
                response, 
                "Corporate actions retrieved successfully"
            );
            return Ok(apiResponse);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var corporateAction = await _repo.GetByIdAsync(id);
            if (corporateAction == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Corporate action not found"));
            
            var response = new CorporateActionResponse
            {
                Id = corporateAction.Id,
                Type = corporateAction.Type.ToString(),
                ExDate = corporateAction.ExDate,
                ParentInstrumentId = corporateAction.ParentInstrumentId,
                ParentInstrumentName = corporateAction.ParentInstrument?.Name ?? string.Empty,
                ChildInstrumentId = corporateAction.ChildInstrumentId,
                ChildInstrumentName = corporateAction.ChildInstrument?.Name,
                RatioNumerator = corporateAction.RatioNumerator,
                RatioDenominator = corporateAction.RatioDenominator,
                CostPercentageAllocated = corporateAction.CostPercentageAllocated
            };
            
            var apiResponse = ApiResponse<CorporateActionResponse>.SuccessResponse(
                response, 
                "Corporate action retrieved successfully"
            );
            return Ok(apiResponse);
        }

        [HttpGet("instrument/{instrumentId:long}")]
        public async Task<IActionResult> GetByInstrumentId(long instrumentId)
        {
            var instrument = await _instrumentRepo.GetByIdAsync(instrumentId);
            if (instrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Instrument not found"));
            
            var corporateActions = await _repo.GetByInstrumentIdAsync(instrumentId);
            
            var response = corporateActions.Select(ca => new CorporateActionResponse
            {
                Id = ca.Id,
                Type = ca.Type.ToString(),
                ExDate = ca.ExDate,
                ParentInstrumentId = ca.ParentInstrumentId,
                ParentInstrumentName = ca.ParentInstrument?.Name ?? string.Empty,
                ChildInstrumentId = ca.ChildInstrumentId,
                ChildInstrumentName = ca.ChildInstrument?.Name,
                RatioNumerator = ca.RatioNumerator,
                RatioDenominator = ca.RatioDenominator,
                CostPercentageAllocated = ca.CostPercentageAllocated
            });
            
            var apiResponse = ApiResponse<IEnumerable<CorporateActionResponse>>.SuccessResponse(
                response, 
                $"Corporate actions for instrument '{instrument.Name}' retrieved successfully"
            );
            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCorporateActionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid corporate action data"));
            
            if (!Enum.TryParse<CorporateActionType>(request.Type, true, out var actionType))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid corporate action type. Valid types are: Split, Bonus, Merger, Demerger, NameChange"
                ));
            
            var parentInstrument = await _instrumentRepo.GetByIdAsync(request.ParentInstrumentId);
            if (parentInstrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Parent instrument not found"));
            
            if (request.ChildInstrumentId.HasValue)
            {
                var childInstrument = await _instrumentRepo.GetByIdAsync(request.ChildInstrumentId.Value);
                if (childInstrument == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Child instrument not found"));
                
                if (request.ParentInstrumentId == request.ChildInstrumentId.Value)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Parent and child instruments cannot be the same"
                    ));
            }
            
            if ((actionType == CorporateActionType.Merger || actionType == CorporateActionType.Demerger) 
                && !request.ChildInstrumentId.HasValue)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"{actionType} requires a child instrument"
                ));
            }
            
            var corporateAction = new CorporateAction
            {
                Type = actionType,
                ExDate = request.ExDate,
                ParentInstrumentId = request.ParentInstrumentId,
                ChildInstrumentId = request.ChildInstrumentId,
                RatioNumerator = request.RatioNumerator,
                RatioDenominator = request.RatioDenominator,
                CostPercentageAllocated = request.CostPercentageAllocated
            };
            
            var created = await _repo.CreateAsync(corporateAction);
            
            var response = new CorporateActionResponse
            {
                Id = created.Id,
                Type = created.Type.ToString(),
                ExDate = created.ExDate,
                ParentInstrumentId = created.ParentInstrumentId,
                ParentInstrumentName = created.ParentInstrument?.Name ?? string.Empty,
                ChildInstrumentId = created.ChildInstrumentId,
                ChildInstrumentName = created.ChildInstrument?.Name,
                RatioNumerator = created.RatioNumerator,
                RatioDenominator = created.RatioDenominator,
                CostPercentageAllocated = created.CostPercentageAllocated
            };
            
            var apiResponse = ApiResponse<CorporateActionResponse>.SuccessResponse(
                response, 
                "Corporate action created successfully"
            );
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, apiResponse);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateCorporateActionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid corporate action data"));
            
            if (!Enum.TryParse<CorporateActionType>(request.Type, true, out var actionType))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid corporate action type. Valid types are: Split, Bonus, Merger, Demerger, NameChange"
                ));
            
            var parentInstrument = await _instrumentRepo.GetByIdAsync(request.ParentInstrumentId);
            if (parentInstrument == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Parent instrument not found"));
            
            if (request.ChildInstrumentId.HasValue)
            {
                var childInstrument = await _instrumentRepo.GetByIdAsync(request.ChildInstrumentId.Value);
                if (childInstrument == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Child instrument not found"));
                
                if (request.ParentInstrumentId == request.ChildInstrumentId.Value)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Parent and child instruments cannot be the same"
                    ));
            }
            
            if ((actionType == CorporateActionType.Merger || actionType == CorporateActionType.Demerger) 
                && !request.ChildInstrumentId.HasValue)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"{actionType} requires a child instrument"
                ));
            }
            
            var corporateAction = new CorporateAction
            {
                Id = id,
                Type = actionType,
                ExDate = request.ExDate,
                ParentInstrumentId = request.ParentInstrumentId,
                ChildInstrumentId = request.ChildInstrumentId,
                RatioNumerator = request.RatioNumerator,
                RatioDenominator = request.RatioDenominator,
                CostPercentageAllocated = request.CostPercentageAllocated
            };
            
            var updated = await _repo.UpdateAsync(id, corporateAction);
            if (updated == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Corporate action not found"));
            
            var response = new CorporateActionResponse
            {
                Id = updated.Id,
                Type = updated.Type.ToString(),
                ExDate = updated.ExDate,
                ParentInstrumentId = updated.ParentInstrumentId,
                ParentInstrumentName = updated.ParentInstrument?.Name ?? string.Empty,
                ChildInstrumentId = updated.ChildInstrumentId,
                ChildInstrumentName = updated.ChildInstrument?.Name,
                RatioNumerator = updated.RatioNumerator,
                RatioDenominator = updated.RatioDenominator,
                CostPercentageAllocated = updated.CostPercentageAllocated
            };
            
            var apiResponse = ApiResponse<CorporateActionResponse>.SuccessResponse(
                response, 
                "Corporate action updated successfully"
            );
            return Ok(apiResponse);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Corporate action not found"));
            
            var apiResponse = ApiResponse<object>.SuccessResponse(
                null, 
                "Corporate action deleted successfully"
            );
            return Ok(apiResponse);
        }
    }
}
