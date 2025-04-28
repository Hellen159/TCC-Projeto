using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Mappings
{
    public class AlunoMapping : IEntityTypeConfiguration<Aluno>
    {
        public void Configure(EntityTypeBuilder<Aluno> builder)
        {
            builder.ToTable("alunos");

            builder.HasKey(a => a.Matricula);

            builder.Property(a => a.Matricula)
                .HasColumnName("matricula")
                .HasColumnType("varchar(9)");


            builder.Property(a => a.NomeAluno)
                .IsRequired()
                .HasColumnName("nome")
                .HasColumnType("varchar(150)");

            builder.Property(a => a.SemestreEntrada)
                .IsRequired()
                .HasColumnName("semestre_entrada")
                .HasColumnType("varchar(7)");

            builder.Property(a => a.HistoricoAnexado)
                .HasColumnName("historico_anexado")
                .HasColumnType("TINYINT(0)");

            builder.Property(a => a.CodigoUser)
            .HasColumnName("user_id")
            .HasColumnType("varchar(255)");


            builder.HasOne(a => a.User)
                .WithOne(u => u.Aluno)
                .HasForeignKey<Aluno>(a => a.CodigoUser);
        }
    }
}
