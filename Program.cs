using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var logger = new LoggerConfiguration().MinimumLevel
    .Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
Log.Logger = logger;
builder.Host.UseSerilog(logger);
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

builder.Services.AddDbContext<IDataContext, DataContext>(options =>
{
    // options.UseSqlite("Data Source=.api.db");
    //options.UseSqlServer("Server=localhost;Database=api;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddSerilog()));
});
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped(typeof(IClaimRepository), typeof(ClaimRepository));
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DbInit>();

var app = builder.Build();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/",
    async (DbInit init, IDataContext dbContext) =>
    {
        await init.Initialize(dbContext);
        return "Hello World!";
    }
);
app.MapGet(
    "/{id:Guid}",
    async (IClaimRepository claimRepository, [FromRoute] Guid id) =>
    {
        var result = await claimRepository.GetWithDebtorsByIdAsync(id);
        return result;
    }
);
app.Run();
