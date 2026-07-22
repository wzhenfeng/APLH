using APLH.Data;
using APLH.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Dapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using APLH.Hubs;

var builder = WebApplication.CreateBuilder(args);


foreach (var modelType in typeof(APLH.Models.User).Assembly.GetTypes()
             .Where(t => t.Namespace == "APLH.Models"))
{
    SqlMapper.SetTypeMap(
        modelType,
        new CustomPropertyTypeMap(
            modelType,
            (type, columnName) => type.GetProperties().FirstOrDefault(prop =>
                prop.Name.Equals(columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
        )
    );
}


builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddHttpClient<EmailService>();



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
app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();