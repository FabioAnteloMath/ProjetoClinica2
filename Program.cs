using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(builder.Configuration["Database:MySql"],
        new MySqlServerVersion(new Version(8, 0, 21)));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

app.MapPost("/paciente", (PacienteRequest pacienteRequest, ApplicationDbContext context) =>
{
    var paciente = new Paciente
    {
        Name = pacienteRequest.Name,
        Code = pacienteRequest.Code,
        Rg = pacienteRequest.Rg,
        Cpf = pacienteRequest.Cpf,
        Endereco = pacienteRequest.Endereco
    };
    context.Pacientes.Add(paciente);
    context.SaveChanges();
    return Results.Created($"/pacientes/{paciente.Id}", paciente.Id);
});

app.MapGet("/pacientes", (ApplicationDbContext context) =>
{
    var pacientes = context.Pacientes.ToList();
    return Results.Ok(pacientes);
});

app.MapGet("/paciente/{name}", ([FromRoute] string name, ApplicationDbContext context) =>
{
    var paciente = context.Pacientes.FirstOrDefault(p => p.Name == name);
    if (paciente != null)
    {
        return Results.Ok(paciente);
    }
    return Results.NotFound();
});

app.MapPut("/paciente/{name}", ([FromRoute] string name, PacienteRequest pacienteRequest, ApplicationDbContext context) =>
{
    var paciente = context.Pacientes.FirstOrDefault(p => p.Name == name);

    if (paciente == null)
    {
        return Results.NotFound();
    }

    paciente.Name = pacienteRequest.Name;
    paciente.Code = pacienteRequest.Code;
    paciente.Rg = pacienteRequest.Rg;
    paciente.Cpf = pacienteRequest.Cpf;
    paciente.Endereco = pacienteRequest.Endereco;

    context.SaveChanges();
    return Results.Ok();
});

app.MapDelete("/paciente/{name}", ([FromRoute] string name, ApplicationDbContext context) =>
{
    var paciente = context.Pacientes.FirstOrDefault(p => p.Name == name);
    if (paciente != null)
    {
        context.Pacientes.Remove(paciente);
        context.SaveChanges();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.MapPost("/procedimento", (ProcedimentoRequest procedimentoRequest, ApplicationDbContext context) =>
{
    // Verifique se o PacienteId e MedicoId existem antes de adicionar um procedimento
    var paciente = context.Pacientes.FirstOrDefault(p => p.Id == procedimentoRequest.PacienteId);
    var medico = context.Medicos.FirstOrDefault(m => m.Id == procedimentoRequest.MedicoId);

    if (paciente == null || medico == null)
    {
        // PacienteId ou MedicoId inválidos, retorne um erro ou uma resposta apropriada.
        return Results.BadRequest("PacienteId ou MedicoId inválidos.");
    }

    var procedimento = new Procedimento
    {
        Code = procedimentoRequest.Code,
        Description = procedimentoRequest.Description,
        Valor = procedimentoRequest.Valor,
        PacienteId = procedimentoRequest.PacienteId,
        MedicoId = procedimentoRequest.MedicoId,
    };

    context.Procedimentos.Add(procedimento);
    context.SaveChanges();
    return Results.Created($"/procedimento/{procedimento.Id}", procedimento.Id);
});

app.MapGet("/procedimento/{Code}", ([FromRoute] string code, ApplicationDbContext context) =>
{
    var procedimento = context.Procedimentos.FirstOrDefault(p => p.Code == code);
    if (procedimento != null)
    {
        return Results.Ok(procedimento);
    }
    return Results.NotFound();
});


app.Run();

public record PacienteRequest(string Name, string Code, string Rg, string Cpf, string Endereco);

public record ProcedimentoRequest(string Code, string Description, decimal Valor, int PacienteId, int MedicoId);

public class Paciente
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Rg { get; set; }
    public string? Cpf { get; set; }
    public string? Endereco { get; set; }
    public ICollection<Procedimento> Procedimentos { get; set; }
}

public static class PacienteList
{
    public static List<Paciente> Pacientes { get; set; }
    public static void Add(Paciente paciente)
    {
        if (Pacientes == null)
            Pacientes = new List<Paciente>();

        Pacientes.Add(paciente);
    }

    public static Paciente GetBy(String name)
    {
        return Pacientes.First(p => p.Name == name);
    }

    internal static void Remove(Paciente pacienteSaved)
    {
        throw new NotImplementedException();
    }
}

public class Medico
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Matricula { get; set; }
    public string? Rg { get; set; }
    public string? Cpf { get; set; }
    public bool Active { get; set; }
    public ICollection<Procedimento> Procedimentos { get; set; }
}

public static class MedicoList
{
    public static List<Medico> Medicos { get; set; }
    public static void Add(Medico medico)
    {
        if (Medicos == null)
            Medicos = new List<Medico>();

        Medicos.Add(medico);
    }

    public static Medico GetBy(String nome)
    {
        return Medicos.First(m => m.Nome == nome);
    }

    internal static void Remove(Medico medicoSaved)
    {
        throw new NotImplementedException();
    }
}

public class Procedimento
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Valor { get; set; }

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; }

    public int MedicoId { get; set; }
    public Medico Medico { get; set; }
}

public static class ProcedimentoList
{
    public static List<Procedimento> Procedimentos { get; set; }
    public static void Add(Procedimento procedimento)
    {
        if (Procedimentos == null)
            Procedimentos = new List<Procedimento>();

        Procedimentos.Add(procedimento);
    }

    public static Procedimento GetBy(String code)
    {
        return Procedimentos.First(p => p.Code == code);
    }

    internal static void Remove(Procedimento procedimentoSaved)
    {
        throw new NotImplementedException();
    }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Procedimento> Procedimentos { get; set; }


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Paciente>().Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Paciente>().Property(p => p.Code).HasMaxLength(20).IsRequired();
        builder.Entity<Paciente>().Property(p => p.Rg).HasMaxLength(20).IsRequired();
        builder.Entity<Paciente>().Property(p => p.Cpf).HasMaxLength(20).IsRequired();
        builder.Entity<Paciente>().Property(p => p.Endereco).HasMaxLength(500).IsRequired(false);

        builder.Entity<Medico>().ToTable("Medicos");
        builder.Entity<Medico>().Property(m => m.Nome).HasMaxLength(120).IsRequired();
        builder.Entity<Medico>().Property(m => m.Matricula).HasMaxLength(20).IsRequired();
        builder.Entity<Medico>().Property(m => m.Rg).HasMaxLength(20).IsRequired();
        builder.Entity<Medico>().Property(m => m.Cpf).HasMaxLength(20).IsRequired();
        builder.Entity<Medico>().Property(m => m.Active).IsRequired();

        builder.Entity<Procedimento>().ToTable("Procedimentos");
        builder.Entity<Procedimento>().Property(p => p.Code).HasMaxLength(120).IsRequired();
        builder.Entity<Procedimento>().Property(p => p.Description).HasMaxLength(200).IsRequired();
        builder.Entity<Procedimento>().Property(p => p.Valor).HasColumnType("decimal(18,2)").IsRequired();

        builder.Entity<Procedimento>()
            .HasOne(p => p.Paciente)
            .WithMany(p => p.Procedimentos)
            .HasForeignKey(p => p.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Procedimento>()
            .HasOne(p => p.Medico)
            .WithMany(m => m.Procedimentos)
            .HasForeignKey(p => p.MedicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}