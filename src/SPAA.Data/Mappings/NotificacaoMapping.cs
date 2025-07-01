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
    public class NotificacaoMapping : IEntityTypeConfiguration<Notificacao>
    {
        public void Configure(EntityTypeBuilder<Notificacao> builder)
        {
            builder.ToTable("notificacoes");

            builder.HasKey(n => n.CodigoNotificacao);

            builder.Property(n => n.CodigoNotificacao)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_notificacao")
                .IsRequired();

            builder.Property(n => n.Mensagem)
                .HasColumnName("mensagem")
                .HasColumnType("varchar(5000)");

            builder.Property(n => n.StatusNotificacao)
                .HasColumnName("status");
        }
    }
}
