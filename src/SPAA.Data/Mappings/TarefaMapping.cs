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
    public class TarefaMapping : IEntityTypeConfiguration<Tarefa>
    {
        public void Configure(EntityTypeBuilder<Tarefa> builder)
        {
            builder.ToTable("tarefas");

            builder.HasKey(t => t.CodigoTarefa);

            builder.Property(t => t.CodigoTarefa)
                .ValueGeneratedOnAdd()
                .HasColumnName("cd_tarefa")
                .IsRequired();
            
            builder.Property(t => t.TipoTarefa)
                .HasColumnName("tipo_tarefa")
                .HasColumnType("varchar(150)");

            builder.HasData(
                new Tarefa { CodigoTarefa = 1, TipoTarefa = "Aula"},
                new Tarefa{ CodigoTarefa = 2, TipoTarefa = "Estagio"}
                );

        }
    }
}
