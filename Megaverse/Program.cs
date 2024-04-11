using System.Net.Http;
using System.Net.Http.Headers;
using Megaverse.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient();
var candidateId = "your_candidate_id"; // Ideally, store this in a secure place like appsettings.json or environment variables
builder.Services.AddSingleton<MegaverseService>(sp =>
    new MegaverseService(sp.GetRequiredService<IHttpClientFactory>().CreateClient(), candidateId));

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

