using APLH.Data;
using APLH.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Dapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Dapper to map snake_case to PascalCase
SqlMapper.SetTypeMap(
    typeof(APLH.Models.QuizQuestion),
    new CustomPropertyTypeMap(
        typeof(APLH.Models.QuizQuestion),
        (type, columnName) => type.GetProperties().FirstOrDefault(prop =>
            prop.Name.Equals(columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
    )
);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EmailService>();


// Register services
builder.Services.AddScoped<SqlRepository>();
builder.Services.AddScoped<LearningService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.Name = "APLHAuth";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();