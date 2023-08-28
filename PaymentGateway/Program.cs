using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Controllers;
using PaymentGateway.Data;
using PaymentGateway.Helpers;
using System.Web.Mvc;
using System.IO;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using PaymentGateway.Models;
using PaymentGateway.DTOs;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PaymentGatewayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentGatewayContext") ?? throw new InvalidOperationException("Connection string 'PaymentGatewayContext' not found.")));


// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);



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



