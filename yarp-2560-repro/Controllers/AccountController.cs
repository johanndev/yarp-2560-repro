using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace yarp_2560_repro.Controllers;

// Despite being decorated with the [AllowAnonymous] attribute, the authorization requirement
// "RolesAuthorizationRequirement:User.IsInRole" is still evaluated IF the authorization policy
// was set in the YARP config.

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    [HttpGet]
    public IActionResult AccessDenied(string returnUrl = "<empty>")
    {
        return new OkObjectResult($"AccessDenied to: {returnUrl}");
    }
}
