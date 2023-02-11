using SampleApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureCosmosDb();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
app.ConfigureEndpoints();
app.Run();
