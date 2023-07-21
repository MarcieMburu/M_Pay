using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;



internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<PaymentGatewayContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentGatewayContext") ?? throw new InvalidOperationException("Connection string 'PaymentGatewayContext' not found.")));

        // Add services to the container.
        builder.Services.AddControllersWithViews();


        // Register AutoMapper here directly with IServiceCollection
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
            pattern: "{controller=Transactions}/{action=Transaction}/{id?}");

        app.Run();
    }
}