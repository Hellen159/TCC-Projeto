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

            builder.HasKey(d => d.CodigoDisciplina);

            builder.Property(d => d.CodigoDisciplina)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_disciplina")
                .IsRequired();

            builder.Property(d => d.NomeDisciplina)
                .HasColumnName("nome_disciplina")
                .HasColumnType("varchar(150)");

            builder.Property(d => d.CargaHoraria)
                .HasColumnName("carga_horaria");
            
        }
    }
}
