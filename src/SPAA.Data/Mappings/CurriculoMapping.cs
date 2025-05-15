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
    public class CurriculoMapping : IEntityTypeConfiguration<Curriculo>
    {
        public void Configure(EntityTypeBuilder<Curriculo> builder)
        {
            builder.ToTable("curriculos");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .IsRequired();

            builder.Property(d => d.NomeDisciplina)
                .ValueGeneratedNever()
                .HasColumnType("varchar(150)")
                .HasColumnName("nome_disciplina");

            builder.Property(d => d.AnoCurriculo)
                .HasColumnName("ano_curriculo")
                .HasColumnType("varchar(7)");

            builder.Property(d => d.TipoDisciplina)
                .HasColumnName("cd_tipo_disciplina");

            builder.Property(d => d.CodigoCurso)
                .HasColumnName("cd_curso");
        }
    }
}
