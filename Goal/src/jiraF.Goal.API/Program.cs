using jiraF.Goal.API.Contracts;
using jiraF.Goal.API.Infrastructure.Data.Contexts;
using jiraF.Goal.API.Infrastructure.Data.Repositories;
using jiraF.Goal.API.Infrastructure.RabbitMQ;
using jiraF.Goal.API.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

ApiKey.Value = builder.Configuration["GoalApiKey"] ?? Environment.GetEnvironmentVariable("GoalApiKey");

builder.Services.AddDbContext<AppDbContext>(options =>
{
#if DEBUG // TODO: Delete this line, now if we do this, tests be broken.
    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
#else
    options.UseInMemoryDatabase("TestData");
    //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "TestConfNamee",
        builder =>
        {
            builder.WithOrigins("https://jiraf-goal.herokuapp.com");
            builder.AllowAnyMethod();
            builder.AllowAnyHeader();
            builder.AllowAnyOrigin();
        });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("GoalApiKey", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "GoalApiKey",
        Description = "Enter GoalApiKey value",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "GoalApiKey" }
            },
            new List<string>()
        }
    });
});
builder.Services.AddTransient<IGoalRepository, GoalRepository>();
builder.Services.AddTransient<ILabelRepository, LabelRepository>();
builder.Services.AddTransient<BunConsumer>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("TestConfNamee");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/ping");

app.Run();
