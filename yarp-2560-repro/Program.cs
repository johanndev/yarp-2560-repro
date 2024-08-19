using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
    cfg.Authority = "https://demo.duendesoftware.com";
    cfg.ClientId = "interactive.confidential";
    cfg.ClientSecret = "secret";
    cfg.ResponseType = OpenIdConnectResponseType.Code;
    cfg.ResponseMode = OpenIdConnectResponseMode.FormPost;

    cfg.Scope.Clear();
    cfg.Scope.Add("openid");
    cfg.Scope.Add("profile");
    cfg.Scope.Add("email");
    // Add any additional scopes you need

    cfg.SaveTokens = true;
    cfg.GetClaimsFromUserInfoEndpoint = true;
    cfg.UsePkce = true;

    cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
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
