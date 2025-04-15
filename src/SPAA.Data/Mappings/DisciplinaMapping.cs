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
                .HasColumnName("cd_disciplina");

            builder.Property(d => d.NomeDisciplina)
                .HasColumnName("nome")
                .HasColumnType("varchar(150)");

            builder.Property(d => d.CargaHoraria)
                .HasColumnName("carga_horaria");


            builder.Property(d => d.CodigoTipoDisciplina)
                .HasColumnName("cd_tipo_disciplina");

            builder.Property(d => d.CodigoCurso)
                .HasColumnName("cd_curso");
            
            builder.Property(d => d.Curriculo)
                .HasColumnName("curriculo");
        }
    }
}
