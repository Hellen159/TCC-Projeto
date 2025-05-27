using System.Collections.Generic;
using System.Threading.Tasks;
using SPAA.Business.Models;
using SPAA.Business.Services;
using Xunit;

namespace SPAA.Business.Tests.Services
{
    public class AulaHorarioServiceTests
    {
        private readonly AulaHorarioService _service = new AulaHorarioService();

        [Fact]
        public async Task ParseHorariosTurma_DeveRetornarListaVazia_SeHorarioTurmaForNuloOuVazio()
        {
            // Act
            var resultado1 = await _service.ParseHorariosTurma(null);
            var resultado2 = await _service.ParseHorariosTurma("");
            var resultado3 = await _service.ParseHorariosTurma("   ");

            // Assert
            Assert.Empty(resultado1);
            Assert.Empty(resultado2);
            Assert.Empty(resultado3);
        }

        [Fact]
        public async Task ParseHorariosTurma_DeveInterpretarCorretamenteUmBlocoSimples()
        {
            // Arrange
            var entrada = "46T6";

            // Act
            var resultado = await _service.ParseHorariosTurma(entrada);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, h => h.DiaSemana == 4 && h.Turno == 'T' && h.Horario == 6);
            Assert.Contains(resultado, h => h.DiaSemana == 6 && h.Turno == 'T' && h.Horario == 6);
        }

        [Fact]
        public async Task ParseHorariosTurma_DeveInterpretarMultiplosHorariosParaMesmosDias()
        {
            // Arrange
            var entrada = "3N45";

            // Act
            var resultado = await _service.ParseHorariosTurma(entrada);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, h => h.DiaSemana == 3 && h.Turno == 'N' && h.Horario == 4);
            Assert.Contains(resultado, h => h.DiaSemana == 3 && h.Turno == 'N' && h.Horario == 5);
        }

        [Fact]
        public async Task ParseHorariosTurma_DeveInterpretarMultiplosBlocosSeparadosPorVirgula()
        {
            // Arrange
            var entrada = "2T5,4N3";

            // Act
            var resultado = await _service.ParseHorariosTurma(entrada);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, h => h.DiaSemana == 2 && h.Turno == 'T' && h.Horario == 5);
            Assert.Contains(resultado, h => h.DiaSemana == 4 && h.Turno == 'N' && h.Horario == 3);
        }

        [Fact]
        public async Task ParseHorariosTurma_DeveIgnorarBlocosInvalidos()
        {
            // Arrange
            var entrada = "2T5,X9"; // "X9" não possui letra de turno válida após os dígitos

            // Act
            var resultado = await _service.ParseHorariosTurma(entrada);

            // Assert
            Assert.Single(resultado);
            Assert.Contains(resultado, h => h.DiaSemana == 2 && h.Turno == 'T' && h.Horario == 5);
        }
    }
}
