using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Tests.Repository
{
    public class CurriculoRepositoryTests
    {
        private MeuDbContext ObterContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MeuDbContext(options);
        }

        [Fact]
        public async Task ObterDisciplinasObrigatoriasPorCurrciulo_DeveRetornarListaCorreta()
        {
            // Arrange
            var contexto = ObterContextoEmMemoria();

            contexto.Set<Curriculo>().AddRange(new List<Curriculo>
        {
            new Curriculo { CodigoCurriculo = 1, NomeDisciplina = "Matemática", AnoCurriculo = "2023", TipoDisciplina = 1, CodigoCurso = 10 },
            new Curriculo { CodigoCurriculo = 2, NomeDisciplina = "História", AnoCurriculo = "2023", TipoDisciplina = 2, CodigoCurso = 10 },
            new Curriculo { CodigoCurriculo = 3, NomeDisciplina = "Física", AnoCurriculo = "2024", TipoDisciplina = 1, CodigoCurso = 10 },
            new Curriculo { CodigoCurriculo = 4, NomeDisciplina = "Química", AnoCurriculo = "2023", TipoDisciplina = 1, CodigoCurso = 10 }
        });

            await contexto.SaveChangesAsync();

            var repo = new CurriculoRepository(contexto);

            // Act
            var resultado = await repo.ObterDisciplinasPorCurrciulo("2023", 1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.All(resultado, c => Assert.Equal("2023", c.AnoCurriculo));
            Assert.All(resultado, c => Assert.Equal(1, c.TipoDisciplina));
        }
    }
}
