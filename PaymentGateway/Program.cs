using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Controllers;
using PaymentGateway.Data;
using PaymentGateway.DTOs;
using System.Web.Mvc;
using System.IO;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PaymentGatewayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentGatewayContext") ?? throw new InvalidOperationException("Connection string 'PaymentGatewayContext' not found.")));


// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient();

//var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
//builder.Services.Configure<ApiSettings>(options => builder.Configuration.GetSection("ApiSettings").Bind(apiSettings));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));



builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

//DI for DbContext

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transactions}/{action=transaction}/{id?}");

app.Run();



