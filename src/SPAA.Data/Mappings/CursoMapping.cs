using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAA.Business.Models;

namespace SPAA.Data.Mappings
{
    public class CursoMapping : IEntityTypeConfiguration<Curso>
    {
        public void Configure(EntityTypeBuilder<Curso> builder)
        {
            builder.ToTable("cursos");

            builder.HasKey(c => c.CodigoCurso);

            builder.Property(c => c.CodigoCurso)
                .HasColumnName("cd_curso");

            builder.Property(c => c.NomeCurso)
                .HasColumnName("nome");

            builder.Property(c => c.CargaHorariaObrigatoria)
                .HasColumnName("carga_horaria_obrigatoria");

            builder.Property(c => c.CargaHorariaOptativa)
                .HasColumnName("carga_horaria_optativa");

            builder.HasData(
                new Curso { CodigoCurso = 1, NomeCurso = "comum", CargaHorariaObrigatoria = 0, CargaHorariaOptativa = 0 },
                new Curso { CodigoCurso = 2, NomeCurso = "software", CargaHorariaObrigatoria = 2580, CargaHorariaOptativa = 900 }
                );


        }
    }
}
