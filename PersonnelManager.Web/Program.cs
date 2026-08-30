using Microsoft.AspNetCore.Authentication.Cookies;
using PersonnelManager.Web.ApiClient;

var builder = WebApplication.CreateBuilder(args);

// --- API client -----------------------------------------------------------
// A typed HttpClient pointed at the REST API; BearerTokenHandler adds the JWT per request.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
                 ?? throw new InvalidOperationException("Missing 'Api:BaseUrl' configuration.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services
    .AddHttpClient<IPersonnelApiClient, PersonnelApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();

// --- Auth (cookie holds the JWT + role claims) ----------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// --- Razor Pages (the whole /Personnel area requires a signed-in user) ----
builder.Services
    .AddRazorPages(options => options.Conventions.AuthorizeFolder("/Personnel"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
