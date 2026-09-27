using FinanSmart.Api.Modules.Admin.Dashboard.DTOs;
using FinanSmart.Api.Modules.Admin.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Admin.Dashboard.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class AdminDashboardController(IAdminDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> Get() => Ok(await service.GetAsync());
}
