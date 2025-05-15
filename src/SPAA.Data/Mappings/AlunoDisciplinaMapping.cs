using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAA.Business.Models;

namespace SPAA.Data.Mappings
{
    public class AlunoDisciplinaMapping : IEntityTypeConfiguration<AlunoDisciplina>
    {
        public void Configure(EntityTypeBuilder<AlunoDisciplina> builder)
        {
            builder.ToTable("alunos_disciplinas");

            builder.HasKey(ad => new { ad.Matricula, ad.NomeDisicplina, ad.Semestre });

            builder.Property(ad => ad.Matricula)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("matricula");

            builder.Property(ad => ad.NomeDisicplina)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("nome_disciplina");

            builder.Property(ad => ad.Semestre)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("semestre");

            builder.Property(ad => ad.Situacao)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("situacao");
        }
    }
}
