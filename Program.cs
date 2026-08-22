using System.ComponentModel.DataAnnotations;
using api_infor_cell.src.Configuration;
using api_infor_cell.src.Middleware;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddEndpointsApiExplorer();
builder.AddBuilderConfiguration();
builder.AddBuilderAuthentication();
builder.AddContext();
builder.AddBuilderServices();
// builder.Services.AddControllers()
// .ConfigureApiBehaviorOptions(options =>
// {
//     options.InvalidModelStateResponseFactory = context =>
//     {
//         var errors = context.ModelState
//             .Where(e => e.Value!.Errors.Count > 0)
//             .Select(e => new {
//                 Field = e.Key,
//                 Message = e.Value!.Errors.First().ErrorMessage,
//                 Order = context.ActionDescriptor.Parameters
//                     .SelectMany(p => p.ParameterType.GetProperties())
//                     .FirstOrDefault(p => p.Name == e.Key)?
//                     .GetCustomAttributes(typeof(DisplayAttribute), false)
//                     .Cast<DisplayAttribute>()
//                     .FirstOrDefault()?.Order ?? 999
//             })
//             .OrderBy(e => e.Order) 
//             .Select(e => new { e.Field, e.Message })
//             .ToList();

//         return new BadRequestObjectResult(new { errors });
//     };
// });

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiLoggerMiddleware>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .Select(e => new {
                Field   = e.Key,
                Message = e.Value!.Errors.First().ErrorMessage,
                Order   = context.ActionDescriptor.Parameters
                    .SelectMany(p => p.ParameterType.GetProperties())
                    .FirstOrDefault(p => p.Name == e.Key)?
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .Cast<DisplayAttribute>()
                    .FirstOrDefault()?.Order ?? 999
            })
            .OrderBy(e => e.Order)
            .Select(e => new { e.Field, e.Message })
            .ToList();

        return new BadRequestObjectResult(new { errors });
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization", 
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "Bearer", 
        BearerFormat = "JWT", 
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

const string ProductionCorsPolicy = "ProductionPolicy";
const string DevelopmentCorsPolicy = "DevelopmentPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: ProductionCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy(name: DevelopmentCorsPolicy, policy  =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build(); 


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(
    app.Environment.IsDevelopment()
        ? DevelopmentCorsPolicy
        : ProductionCorsPolicy
);


var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseMiddleware<CompanyQueryMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
