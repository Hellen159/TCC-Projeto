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

            // Chave primária composta
            builder.HasKey(ad => new { ad.Matricula, ad.CodigoDisciplina, ad.Semestre });

            // Propriedades com nomes de coluna
            builder.Property(ad => ad.Matricula)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("matricula");

            builder.Property(ad => ad.CodigoDisciplina)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("codigo_disciplina");

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
