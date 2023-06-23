using System.Reflection;
using DataFactoryAutomation;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Context;
using WebAPI.Data.Database;
using WebAPI.Data.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ConfigContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConfigConnection"))
);

// Add services to the container.
var azureConfiguration = builder.Configuration.GetSection("Azure").Get<AzureConfiguration>();
builder.Services.AddDataFactoryService(azureConfiguration);

builder.Services.AddScoped<ConfigService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensures the database is populated with demo related data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var configContext = services.GetRequiredService<ConfigContext>();
    configContext.Database.EnsureCreated();
    // Add services to the container.
    var salesforceConfig = builder.Configuration.GetSection("Salesforce").Get<SalesforceConfig>();
    var azureSqlConfig = builder.Configuration.GetSection("AzureSql").Get<AzureSqlConfig>();
    DbInitializer.Initialize(configContext, salesforceConfig, azureSqlConfig);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();