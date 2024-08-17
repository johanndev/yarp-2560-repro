using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(cfg =>
{
    cfg.MapInboundClaims = false;
    cfg.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    cfg.ResponseType = "code token id_token";
    cfg.Authority = builder.Configuration["Adfs:Authority"];
    cfg.ClientId = builder.Configuration["Adfs:ClientId"];
    cfg.UseTokenLifetime = true;

    cfg.UsePkce = true;

    cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        RoleClaimType = "role",
        NameClaimType = "upn"
    };

    cfg.Events.OnTokenValidated = ctx =>
    {
        // Stand-in for more complex logic that fetches roles from an external service
        ICollection<Claim> claims = [
            new Claim("role", "MyRole", ClaimValueTypes.String, "MyIssuer")
        ];

        // Uncomment the next line to add the necessary role that is required by the default
        // auth policy
        //ctx.Principal?.AddIdentity(new ClaimsIdentity(claims, null, "name", "role"));
        return Task.CompletedTask;
    };
});

var authorizationBuilder = builder.Services.AddAuthorizationBuilder();

authorizationBuilder.SetDefaultPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .RequireRole("MyRole")
    .Build());

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

app.Run();
