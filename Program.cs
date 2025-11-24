using BuildToolService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddIniFile("config.ini", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBuildToolService, BuildToolService.BuildToolService>();

Data.Init(builder.Environment.ContentRootPath, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
