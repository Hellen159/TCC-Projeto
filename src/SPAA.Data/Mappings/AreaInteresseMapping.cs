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
                    NomeAreaInteresse = "Desenvolvimento Web",
                    Descricao = "Interesse no desenvolvimento de aplicações frontend/backend, APIs, frameworks modernos."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 2,
                    NomeAreaInteresse = "Inteligência Artificial",
                    Descricao = "Interesse em machine learning, redes neurais, NLP, etc."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 3,
                    NomeAreaInteresse = "Sistemas e Baixo Nível",
                    Descricao = "Interesse em sistemas operacionais, compiladores, engenharia reversa, infraestrutura."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 4,
                    NomeAreaInteresse = "Computação Teórica",
                    Descricao = "Interesse em fundamentos da computação, autômatos, lógica formal, complexidade."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 5,
                    NomeAreaInteresse = "Engenharia de Software",
                    Descricao = "Interesse em requisitos, documentação, gestão de projetos, qualidade de software."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 6,
                    NomeAreaInteresse = "Desempenho e Algoritmos",
                    Descricao = "Interesse na criação de algoritmos otimizados e análise de performance."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 7,
                    NomeAreaInteresse = "Competição e Programação",
                    Descricao = "Interesse em maratonas, hackathons, resolução de problemas competitivos."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 8,
                    NomeAreaInteresse = "Segurança e Hacking Ético",
                    Descricao = "Interesse em segurança da informação, criptografia, testes de invasão."
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 9,
                    NomeAreaInteresse = "Desenvolvimento Profissional",
                    Descricao = ""
                },
                new AreaInteresse
                {
                    CodigoAreaInteresse = 10,
                    NomeAreaInteresse = "Desenvolvimento Acadêmico / Bem-estar",
                    Descricao = ""
                }
            );
        }
    }
}
