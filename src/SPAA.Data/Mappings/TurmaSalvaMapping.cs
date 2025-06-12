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
    public class TurmaSalvaMapping : IEntityTypeConfiguration<TurmaSalva>
    {
        public void Configure(EntityTypeBuilder<TurmaSalva> builder)
        {
            builder.ToTable("turmas_salvas");

            builder.HasKey(ts => ts.CodigoTurmaSalva );

            builder.Property(ts => ts.CodigoTurmaSalva)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_turma_salva")
                .IsRequired();

            builder.Property(ts => ts.CodigoUnicoTurma)
                .HasColumnName("cd_turma_unico");

            builder.Property(ts => ts.Matricula)
                .HasColumnName("matricula")
                .HasColumnType("varchar(9)");

            builder.Property(ts => ts.Horario)
                .HasColumnName("horario")
                .HasColumnType("varchar(20)");

            builder.Property(ts => ts.CodigoDisciplina)
                .HasColumnName("cd_disciplina")
                .HasColumnType("varchar(50)");
        }
    }
}
