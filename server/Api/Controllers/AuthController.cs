using DataAccess.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service;
using Service.Auth.Dto;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<LoginResponse> Login(
        [FromServices] SignInManager<User> signInManager,
        [FromServices] IValidator<LoginRequest> validator,
        [FromBody] LoginRequest data
    )
    {
        //Validate LoginRequest
        var validationResult = await validator.ValidateAsync(data);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        //attempt to authenticate
        var result = await signInManager.PasswordSignInAsync(data.Email, data.Password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw new AuthenticationError("invalid login attempt.");
        }
        
        //return login response on succes
        return new LoginResponse
        {
            Message = data.Email
        };
    }

    [HttpPost]
    [Route("register")]
    public async Task<RegisterResponse> Register(
        IOptions<AppOptions> options,
        [FromServices] UserManager<User> userManager,
        [FromServices] IValidator<RegisterRequest> validator,
        [FromBody] RegisterRequest data
    )
    {
        //Validate RegisterRequest
        var validationResult = await validator.ValidateAsync(data);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        //Create new User object
        var user = new User
        {
            UserName = data.Email,
            Email = data.Email
        };
        
        //attempt to create a new user
        var result = await userManager.CreateAsync(user, data.Password);
        if (!result.Succeeded)
        {
            throw new ValidationError(
                result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description })
            );
        }
        
        //add role to the newly created user
        var roleResult = await userManager.AddToRoleAsync(user, Role.Reader);
        if (!roleResult.Succeeded)
        {
            throw new ValidationError(
                result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description })
            );
        }
        
        //Return RegisterResponse
        return new RegisterResponse(
            user.Email, 
            user.UserName
        );

    }

    [HttpPost]
    [Route("logout")]
    public async Task<IResult> Logout([FromServices] SignInManager<User> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    [HttpGet]
    [Route("userinfo")]
    public async Task<AuthUserInfo> UserInfo([FromServices] UserManager<User> userManager)
    {
        var username = (HttpContext.User.Identity?.Name) ?? throw new AuthenticationError("Could not authenticate");
        var user = await userManager.FindByNameAsync(username) ?? throw new AuthenticationError("User allready exists");
        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(Role.Admin);
        var canPublish = roles.Contains(Role.Editor) || isAdmin;
        return new AuthUserInfo(username, isAdmin, canPublish);
    }
}
