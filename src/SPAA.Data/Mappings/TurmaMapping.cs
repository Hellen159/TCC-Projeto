using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Mappings
{
    public class TurmaMapping : IEntityTypeConfiguration<Turma>
    {
        public void Configure(EntityTypeBuilder<Turma> builder)
        {
            builder.ToTable("turmas");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .IsRequired();

            builder.Property(d => d.CodigoTurma)
                .ValueGeneratedNever()
                .HasColumnName("cd_turma")
                .HasColumnType("varchar(10)");

            builder.Property(d => d.NomeProfessor)
                .HasColumnName("nome_professor")
                .HasColumnType("varchar(150)");

            builder.Property(d => d.Semestre)
                .HasColumnName("semestre")
                .HasColumnType("varchar(7)");


            builder.Property(d => d.Capacidade)
                .HasColumnName("capacidade");

            builder.Property(d => d.CodigoDisciplina)
                .HasColumnName("cd_disciplina")
                .HasColumnType("varchar(10)");

            builder.Property(d => d.Horario)
                .HasColumnName("horario")
                .HasColumnType("varchar(20)");

            //builder.HasOne(t => t.Disciplina)  // Relacionamento com Disciplina
            //    .WithMany(d => d.Turmas)  // Disciplina pode ter muitas Turmas
            //    .HasForeignKey(t => t.CodigoDisciplina)  // Chave estrangeira em Turma
            //    .HasPrincipalKey(d => d.CodigoDisciplina);  // Relacionamento com Codigo da Disciplina
        }
    }
}
