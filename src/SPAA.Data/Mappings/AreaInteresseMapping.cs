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
    public class AreaInteresseMapping : IEntityTypeConfiguration<AreaInteresse>
    {
        public void Configure(EntityTypeBuilder<AreaInteresse> builder)
        {
            builder.ToTable("area_interesse");

            builder.HasKey(ai => ai.CodigoAreaInteresse);

            builder.Property(ai => ai.CodigoAreaInteresse)
                .HasColumnName("cd_area_interesse");

            builder.Property(ai => ai.NomeAreaInteresse)
                .HasColumnType("varchar(150)")
                .HasColumnName("nome");

            builder.Property(ai => ai.Descricao)
                .HasColumnType("varchar(255)")
                .HasColumnName("descricao");

            builder.HasData(
                new AreaInteresse
                {
                    CodigoAreaInteresse = 1,
                    NomeAreaInteresse = "Programação e Infraestrutura Técnica",
                    Descricao = "Construção de código, uso de linguagens de programação, redes, sistemas operacionais, bancos de dados e arquitetura de sistemas."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 2,
                    NomeAreaInteresse = "Projeto e Análise de Software",
                    Descricao = "Relacionado à análise de problemas, levantamento de requisitos, design de soluções, experiência do usuário e modelagem."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 3,
                    NomeAreaInteresse = "Qualidade, Testes e Manutenção",
                    Descricao = "Inclui atividades de verificação e validação de software, testes automatizados, controle de qualidade e manutenção de sistemas."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 4,
                    NomeAreaInteresse = "Gestão de Produtos e Processos",
                    Descricao = "Envolve definição de escopo, métricas, acompanhamento de equipes e organização de processos de desenvolvimento."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 5,
                    NomeAreaInteresse = "Fundamentos Matemáticos e Computacionais",
                    Descricao = "Abrange raciocínio lógico, estatística, pesquisa operacional, linguagens formais e construção de algoritmos."
                }
            );
        }
    }
}
