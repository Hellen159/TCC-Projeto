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
    public class TipoDisciplinaMapping : IEntityTypeConfiguration<TipoDisciplina>
    {
        public void Configure(EntityTypeBuilder<TipoDisciplina> builder)
        {
            builder.ToTable("tipos_disciplinas");

            builder.HasKey(td => td.CodigoTipoDisiciplina);

            builder.Property(td => td.CodigoTipoDisiciplina)
                .HasColumnName("cd_tipo_disciplina")
                       .ValueGeneratedNever();

            builder.Property(td => td.NomeTipoDisciplina)
                .HasColumnName("nome");

            builder.HasData(
                new TipoDisciplina { CodigoTipoDisiciplina = 1, NomeTipoDisciplina = "Obrigatoria" },
                new TipoDisciplina {CodigoTipoDisiciplina = 2, NomeTipoDisciplina = "Optativa" },
                new TipoDisciplina {CodigoTipoDisiciplina = 3, NomeTipoDisciplina = "ModuloLivre" }
            );

        }
    }
}
