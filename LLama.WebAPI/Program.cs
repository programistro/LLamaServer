using LLama.WebApi.Hubs;
using LLama.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<StatefulChatService>();
builder.Services.AddScoped<StatelessChatService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISessionRepository, MemorySessionRepository>();
builder.Services.AddSingleton<LlamaService>();


var app = builder.Build();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();
// app.MapStaticAssets();
app.UseStaticFiles();
app.MapHub<AiHub>("/aiHub");
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

app.Run();
