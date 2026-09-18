using FinanSmart.Api.Common.Exceptions;using FinanSmart.Api.Modules.Investments.Rates.DTOs;using FinanSmart.Api.Modules.Investments.Rates.Interfaces;using Microsoft.AspNetCore.Mvc;
namespace FinanSmart.Api.Modules.Investments.Rates.Controllers;
[ApiController][Route("api/investment-rates")]
public class InvestmentRatesController(IInvestmentRateService service):ControllerBase
{
 [HttpGet] public async Task<ActionResult<IReadOnlyCollection<InvestmentRateDto>>> GetAll()=>Ok(await service.GetAllAsync());
 [HttpGet("{id:guid}")] public async Task<ActionResult<InvestmentRateDto>> GetById(Guid id){var x=await service.GetByIdAsync(id);return x is null?NotFound():Ok(x);}
 [HttpGet("product/{investmentProductId:guid}")] public async Task<ActionResult<IReadOnlyCollection<InvestmentRateDto>>> GetByProduct(Guid investmentProductId)=>Ok(await service.GetByProductAsync(investmentProductId));
 [HttpGet("applicable")] public async Task<ActionResult<InvestmentRateDto>> Applicable(Guid investmentProductId,decimal amount,int termDays){var x=await service.GetApplicableRateAsync(investmentProductId,amount,termDays);return x is null?NotFound():Ok(x);}
 [HttpPost] public async Task<ActionResult<InvestmentRateDto>> Create(CreateInvestmentRateDto dto){try{var x=await service.CreateAsync(dto);return CreatedAtAction(nameof(GetById),new{id=x.Id},x);}catch(InvestmentProductNotFoundException e){return NotFound(new{message=e.Message});}catch(InvestmentRateOverlapException e){return Conflict(new{message=e.Message});}catch(ArgumentException e){return BadRequest(new{message=e.Message});}}
 [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id,UpdateInvestmentRateDto dto){try{return await service.UpdateAsync(id,dto)?NoContent():NotFound();}catch(InvestmentProductNotFoundException e){return NotFound(new{message=e.Message});}catch(InvestmentRateOverlapException e){return Conflict(new{message=e.Message});}catch(ArgumentException e){return BadRequest(new{message=e.Message});}}
 [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id)=>await service.DeleteAsync(id)?NoContent():NotFound();
}
