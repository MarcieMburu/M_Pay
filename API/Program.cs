using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.Extensions.Configuration;


using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Configuration;
using API.Attributes;
using PaymentGateway.Data;
using PaymentGateway.Helpers;
using PaymentGateway.Controllers;
using PaymentGateway.Models;
using PaymentGateway.DTOs;
using Microsoft.Extensions.Options;

using API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentGatewayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentGatewayContext") ?? throw new InvalidOperationException("Connection string 'PaymentGatewayContext' not found.")));
// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped< ZamupayService>();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped(sp => sp.GetRequiredService<IOptions<ApiSettings>>().Value);

//builder.Services.AddHostedService<MessageService>();


builder.Services.AddVersionedApiExplorer(c =>
{
    c.GroupNameFormat = "'v'VVV";
    c.SubstituteApiVersionInUrl = true;
    c.AssumeDefaultVersionWhenUnspecified = true;
    c.DefaultApiVersion = new ApiVersion(1, 0);
});
builder.Services.AddApiVersioning(c =>
{
    c.ReportApiVersions = true;
    c.AssumeDefaultVersionWhenUnspecified = true;
    c.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddAuthentication("BasicAuthentication").
            AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<Customer>(); 

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(new Uri("rabbitmq://localhost" ), h => {
//            h.Username("guest");
//            h.Password("guest");
//            });
//        cfg.ConfigureEndpoints(context); 
//    });
//});

//builder.Services.AddMassTransitHostedService(); 



var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ApplicationMapper());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddSwaggerGen(c =>
{
    
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API",
        Description = "An ASP.NET Core Web API for making payments",

    });
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Auth Header"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "basic"
            }
        },
        new string[] { }
    }
});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    });
}
app.UseRouting();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    }
);

//app.MapUserEndpoints();
app.Run();
