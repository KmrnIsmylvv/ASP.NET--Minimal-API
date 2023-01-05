using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MinAPI.Data;
using MinAPI.Dtos;
using MinAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqlConnectionBuilder = new SqlConnectionStringBuilder();

sqlConnectionBuilder.ConnectionString = builder.Configuration.GetConnectionString("SQLDbConnection");
sqlConnectionBuilder.UserID = builder.Configuration["UserId"];
sqlConnectionBuilder.Password = builder.Configuration["Password"];

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(sqlConnectionBuilder.ConnectionString);
});

builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("api/v1/commands", async (ICommandRepo repo, IMapper mapper) =>
{
    var commands = await repo.GetAllCommands();
    return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
});

app.MapGet("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id) =>
{
    var command = await repo.GetCommandById(id);
    if (command != null)
    {
        return Results.Ok(mapper.Map<CommandReadDto>(command));
    }

    return Results.NotFound();
});

app.MapPost("api/v1/commands", async (ICommandRepo repo, IMapper mapper,
    CommandCreateDto commandCreateDto) =>
{
    var commandModel = mapper.Map<Command>(commandCreateDto);

    await repo.CreateCommand(commandModel);
    await repo.SaveChanges();

    var commandReadDto = mapper.Map<CommandReadDto>(commandModel);

    return Results.Created($"api/v1/commands/{commandReadDto.Id}", commandReadDto);
});

app.MapPut("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id,
    CommandUpdateDto commandUpdateDto) =>
{
    var command = await repo.GetCommandById(id);

    if (command is null)
        return Results.NotFound();

    mapper.Map(commandUpdateDto, command);

    await repo.SaveChanges();

    return Results.NoContent();
});

app.MapDelete("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id) =>
{
    var command = await repo.GetCommandById(id);

    if (command is null)
        return Results.NotFound();

    repo.DeleteCommand(command);

    await repo.SaveChanges();

    return Results.NoContent();
});

app.Run();