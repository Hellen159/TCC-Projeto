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
    public class AreaInteresseAlunoMapping : IEntityTypeConfiguration<AreaInteresseAluno>
    {
        public void Configure(EntityTypeBuilder<AreaInteresseAluno> builder)
        {
            builder.ToTable("area_interesse_aluno");

            builder.HasKey(aia => aia.CodigoAreaInteresseAluno);
            
            builder.Property(ai => ai.CodigoAreaInteresseAluno)
                .HasColumnName("cd_area_interesse_aluno");

            builder.Property(ai => ai.CodigoAreaInteresse)
                .HasColumnName("cd_area_interesse");

            builder.Property(ai => ai.Matricula)
                .HasColumnType("varchar(9)")
                .HasColumnName("matricula");

            builder.Property(ai => ai.NivelInteresse)
                .HasColumnType("varchar(100)")
                .HasColumnName("nivel_interesse");
        }
    }
}
