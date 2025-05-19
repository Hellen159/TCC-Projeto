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
    public class PreRequisitoMapping : IEntityTypeConfiguration<PreRequisito>
    {
        public void Configure(EntityTypeBuilder<PreRequisito> builder)
        {
            builder.ToTable("pre_requisitos");

            builder.HasKey(d => d.CodigoPreRequisito);

            builder.Property(d => d.CodigoPreRequisito)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_pre_requisito")
                .IsRequired();

            builder.Property(d => d.NomeDisciplina)
                .ValueGeneratedNever()
                .HasColumnType("varchar(150)")
                .HasColumnName("nome_disciplina");

            builder.Property(d => d.PreRequisitoLogico)
                .HasColumnName("pre_requisito_logico")
                .HasColumnType("varchar(1500)");
        }
    }
}
