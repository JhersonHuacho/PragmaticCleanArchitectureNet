using Bookify.Api.Extensions;
using Bookify.Application;
using Bookify.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region config serilog logger
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
	loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});
#endregion

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	app.ApplyMigration();

	app.SeedData();
}

app.UseHttpsRedirection();

#region serilog

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

#endregion

#region config Authentication
app.UseAuthorization();

app.UseAuthorization();
#endregion
app.UseCustomExceptionHandler();

app.MapControllers();

app.Run();
