using LudzValorant.Constants;
using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Handle.HandleEmail;
using LudzValorant.ImplementService;
using LudzValorant.InterfaceService;
using LudzValorant.Internal;
using LudzValorant.Payloads.Mappers;
using LudzValorant.Repositories.ImplementRepositories;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.ImplementServices;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PuppeteerSharp;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString
    (Constant.AppSettingKeys.DEFAULT_CONNECTION)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5173")  // Thay thế bằng URL của frontend
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();  // Cho phép gửi cookies
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("WarehouseTasks", policy =>
        policy.RequireRole("WarehouseOperator", "Admin"));
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddHostedService<ExpiredAccountCleaner>();


builder.Services.AddScoped<IDbContext, ApplicationDbContext>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISkinImporterService, SkinImporterService>();
builder.Services.AddScoped<IUserService, UserService>();



builder.Services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
builder.Services.AddScoped<IBaseRepository<UserRole>, BaseRepository<UserRole>>();
builder.Services.AddScoped<IBaseRepository<Role>, BaseRepository<Role>>();
builder.Services.AddScoped<IBaseRepository<ConfirmEmail>, BaseRepository<ConfirmEmail>>();
builder.Services.AddScoped<IBaseRepository<RefreshToken>, BaseRepository<RefreshToken>>();
builder.Services.AddScoped<IBaseRepository<Account>, BaseRepository<Account>>();
builder.Services.AddScoped<IBaseRepository<Skin>, BaseRepository<Skin>>();
builder.Services.AddScoped<IBaseRepository<Product>, BaseRepository<Product>>();
builder.Services.AddScoped<IBaseRepository<Purchase>, BaseRepository<Purchase>>();
builder.Services.AddScoped<IBaseRepository<InstallmentSchedule>, BaseRepository<InstallmentSchedule>>();
builder.Services.AddScoped<IBaseRepository<AccountSkin>, BaseRepository<AccountSkin>>();
builder.Services.AddScoped<IBaseRepository<Weapon>, BaseRepository<Weapon>>();
builder.Services.AddScoped<IBaseRepository<Tier>, BaseRepository<Tier>>();
builder.Services.AddScoped<IBaseRepository<Agent>, BaseRepository<Agent>>();
builder.Services.AddScoped<IBaseRepository<GunBuddy>, BaseRepository<GunBuddy>>();
builder.Services.AddScoped<IBaseRepository<Contract>, BaseRepository<Contract>>();
builder.Services.AddScoped<IBaseRepository<AccountContract>, BaseRepository<AccountContract>>();
builder.Services.AddScoped<IBaseRepository<AccountAgent>, BaseRepository<AccountAgent>>();
builder.Services.AddScoped<IBaseRepository<AccountGunBuddy>, BaseRepository<AccountGunBuddy>>();


builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();



builder.Services.AddScoped<UserConverter>();
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "BookStoreManagement", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Vui lòng nhập token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedDatabaseAsync(app.Services);
}
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Upload", "Avatar")), // Thư mục chứa các tệp ảnh avatar
    RequestPath = "/images/avatars"  // Đường dẫn yêu cầu để truy cập các ảnh này
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Upload", "Product")), // Thư mục chứa các tệp ảnh avatar
    RequestPath = "/images/products"  // Đường dẫn yêu cầu để truy cập các ảnh này
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
