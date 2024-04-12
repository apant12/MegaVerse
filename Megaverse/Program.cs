using System.Net.Http;
using System.Net.Http.Headers;
using Megaverse.Service;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient();

builder.Services.AddSingleton<MegaverseService>(sp =>
    new MegaverseService(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<ILogger<MegaverseService>>(),
        "3ade151f-3c7d-4dd3-8588-2d197a3c0565" // Replace with your actual candidate ID
    ));

builder.Services.AddSingleton<ComethService>(sp =>
    new ComethService(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<ILogger<ComethService>>(),
        "3ade151f-3c7d-4dd3-8588-2d197a3c0565" // Replace with your actual candidate ID
    ));

builder.Services.AddSingleton<SoloonService>(sp =>
    new SoloonService(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<ILogger<SoloonService>>(),
        "3ade151f-3c7d-4dd3-8588-2d197a3c0565" // Replace with your actual candidate ID
    ));

builder.Services.AddControllers();
// Other service configurations...

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

