// Crie esta classe em SPAA.Data.Mappings ou em uma pasta de Teste separada
// Ex: SPAA.Data.Tests.Mappings/TesteEntityMapping.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAA.Business.Models; // Ou SPAA.Data.Tests.Models

namespace SPAA.Data.Mappings // Ou SPAA.Data.Tests.Mappings
{
    public class TesteEntityMapping : IEntityTypeConfiguration<TesteEntity>
    {
        public void Configure(EntityTypeBuilder<TesteEntity> builder)
        {
            builder.ToTable("test_entities");

            builder.HasKey(t => t.CodigoTesteEntity);

            builder.Property(t => t.CodigoTesteEntity)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_test_entity")
                .IsRequired();

            builder.Property(t => t.Nome)
                .HasColumnName("nome")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Descricao)
                .HasColumnName("descricao")
                .HasColumnType("varchar(255)");
        }
    }
}