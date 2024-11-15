using FlAdmin.Common.Models.Auth;
using FlAdmin.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/admin")]

[AdminAuthorize(Role.SuperAdmin)]
public class AdminController : ControllerBase
{
    
    
}