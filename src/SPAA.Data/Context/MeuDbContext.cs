﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;

namespace SPAA.Data.Context
{
    public class MeuDbContext : IdentityDbContext<ApplicationUser>
    {
        public MeuDbContext(DbContextOptions<MeuDbContext> options) : base(options) { }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }
        public DbSet<TipoDisciplina> TipoDisciplinas{ get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<AlunoDisciplina> AlunoDisciplinas { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<PreRequisito> PreRequisitos { get; set; }
        public DbSet<TarefaAluno> TarefasAlunos { get; set; } // Add this
        public DbSet<TurmaSalva> TurmasSalvas { get; set; } // Add this line
        public DbSet<TesteEntity> TesteEntities { get; set; } // Adicione esta linha



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //esqueci de mapear alguma coisa e nao entre como n varchar max
            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //    .SelectMany(e => e.GetProperties()
            //    .Where(p => p.ClrType == typeof(string))))
            //{
            //    property.SetColumnType("varchar(100)");
            //}

            //mapea as entidades automaticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeuDbContext).Assembly);

            //evita a delecao em cascata das entidades relacionadas
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())) relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(modelBuilder);
        }
    }
}
