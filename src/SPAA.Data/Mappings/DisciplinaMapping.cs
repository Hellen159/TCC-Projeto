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
    public class DisciplinaMapping : IEntityTypeConfiguration<Disciplina>
    {
        public void Configure(EntityTypeBuilder<Disciplina> builder)
        {
            builder.ToTable("disciplinas");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .IsRequired();

            builder.Property(d => d.CodigoDisciplina)
                .ValueGeneratedNever()
                .HasColumnType("varchar(20)")
                .HasColumnName("cd_disciplina");

            builder.Property(d => d.NomeDisciplina)
                .HasColumnName("nome")
                .HasColumnType("varchar(150)");

            builder.Property(d => d.CargaHoraria)
                .HasColumnName("carga_horaria");
            
            builder.Property(d => d.Ativa)
                .HasColumnName("ativa")
                .HasColumnType("varchar(10)");

            builder.Property(d => d.CodigoEquivalencia)
                .HasColumnType("varchar(200)")
                .HasColumnName("cd_disciplina_equivalente")
                .IsRequired(false);
        }
    }
}
