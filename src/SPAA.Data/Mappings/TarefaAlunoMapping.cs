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
    public class TarefaAlunoMapping : IEntityTypeConfiguration<TarefaAluno>
    {
        public void Configure(EntityTypeBuilder<TarefaAluno> builder)
        {
            builder.ToTable("tarefas_alunos");

            builder.HasKey(ta => ta.CodigoTarefaAluno);

            builder.Property(ta => ta.CodigoTarefaAluno)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_tarefa_aluno")
                .IsRequired();

            builder.Property(ta => ta.NomeTarefa)
                .HasColumnName("nome_tarefa")
                .HasColumnType("varchar(255)");

            builder.Property(ta => ta.Matricula)
                .HasColumnName("matricula")
                .HasColumnType("varchar(9)");

            builder.Property(ta => ta.Horario)
                .HasColumnName("horario")
                .HasColumnType("varchar(20)");

        }
    }
}
