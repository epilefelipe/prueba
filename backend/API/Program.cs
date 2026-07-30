using System.Data;
using Dapper;
using FluentValidation;
using Microsoft.Data.Sqlite;
using TicketManager.API.Middleware;
using TicketManager.Application.Interfaces;
using TicketManager.Application.Services;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;
using TicketManager.Infrastructure.Repositories;

SqlMapper.AddTypeHandler(new EnumStringHandler<Priority>());
SqlMapper.AddTypeHandler(new EnumStringHandler<Status>());
SqlMapper.AddTypeHandler(new GuidStringHandler());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TicketManager API", Version = "v1" });
});

var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
connection.Execute(@"
    CREATE TABLE IF NOT EXISTS Ticket (
        Id          TEXT PRIMARY KEY,
        Title       TEXT NOT NULL,
        Description TEXT NOT NULL,
        Priority    TEXT NOT NULL,
        Status      TEXT NOT NULL,
        CreatedAt   TEXT NOT NULL,
        UpdatedAt   TEXT NOT NULL,
        CreatedBy   TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS Comment (
        Id          TEXT PRIMARY KEY,
        TicketId    TEXT NOT NULL,
        Text        TEXT NOT NULL,
        CreatedAt   TEXT NOT NULL,
        CreatedBy   TEXT NOT NULL,
        FOREIGN KEY (TicketId) REFERENCES Ticket(Id) ON DELETE CASCADE
    );
    CREATE TABLE IF NOT EXISTS [User] (
        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
        Email       TEXT NOT NULL UNIQUE,
        DisplayName TEXT NOT NULL
    );
");

connection.Execute(@"
    INSERT OR IGNORE INTO [User] (Email, DisplayName) VALUES
        ('user@example.com', 'Usuario Demo'),
        ('admin@example.com', 'Admin'),
        ('operator@logistica.com', 'Operador Logística')
");

builder.Services.AddSingleton<IDbConnection>(connection);

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<TicketService>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();
