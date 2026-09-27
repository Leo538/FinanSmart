using FinanSmart.Api.Common.Exceptions;using FinanSmart.Api.Modules.Investments.Products.DTOs;using FinanSmart.Api.Modules.Investments.Products.Interfaces;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Authorization;
namespace FinanSmart.Api.Modules.Investments.Products.Controllers;
[ApiController][Route("api/investment-products")]
public class InvestmentProductsController(IInvestmentProductService service):ControllerBase
{
 [HttpGet] public async Task<ActionResult<IReadOnlyCollection<InvestmentProductDto>>> GetAll()=>Ok(await service.GetAllAsync());
 [HttpGet("{id:guid}")] public async Task<ActionResult<InvestmentProductDto>> GetById(Guid id){var x=await service.GetByIdAsync(id);return x is null?NotFound():Ok(x);}
 [Authorize(Roles="Admin")][HttpPost] public async Task<ActionResult<InvestmentProductDto>> Create(CreateInvestmentProductDto dto){try{var x=await service.CreateAsync(dto);return CreatedAtAction(nameof(GetById),new{id=x.Id},x);}catch(InvestmentProductNameConflictException e){return Conflict(new{message=e.Message});}catch(ArgumentException e){return BadRequest(new{message=e.Message});}}
 [Authorize(Roles="Admin")][HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id,UpdateInvestmentProductDto dto){try{return await service.UpdateAsync(id,dto)?NoContent():NotFound();}catch(InvestmentProductNameConflictException e){return Conflict(new{message=e.Message});}catch(ArgumentException e){return BadRequest(new{message=e.Message});}}
 [Authorize(Roles="Admin")][HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id)=>await service.DeleteAsync(id)?NoContent():NotFound();
}
