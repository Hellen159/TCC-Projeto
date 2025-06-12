using Moq;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Business.Tests.Services
{
    public class TurmaServiceTests
    {
        private readonly Mock<IAulaHorarioService> _aulaHorarioServiceMock;
        private readonly Mock<ITurmaRepository> _turmaRepositoryMock;
        private readonly TurmaService _turmaService;

        public TurmaServiceTests()
        {
            _aulaHorarioServiceMock = new Mock<IAulaHorarioService>();
            _turmaRepositoryMock = new Mock<ITurmaRepository>();
            _turmaService = new TurmaService(_aulaHorarioServiceMock.Object, _turmaRepositoryMock.Object);
        }

        [Fact]
        public async Task BuscarTurmasCompativeis_DeveRetornarApenasTurmasCompativeis()
        {
            // Arrange
            var nomeDisciplina = "Algoritmos";

            var preferencias = new List<AulaHorario>
        {
            new AulaHorario { DiaSemana = 1, Turno = 'M', Horario = 1 } // Segunda, Matutino, 1º horário
        };

            var turma1 = new Turma { CodigoTurmaUnico = 1, NomeDisciplina = "Algoritmos", Horario = "abc" };
            var turma2 = new Turma { CodigoTurmaUnico = 2, NomeDisciplina = "Algoritmos", Horario = "def" };

            _turmaRepositoryMock.Setup(r => r.TurmasDisponiveisPorDisciplina(nomeDisciplina))
                .ReturnsAsync(new List<Turma> { turma1, turma2 });

            _aulaHorarioServiceMock.Setup(s => s.ParseHorariosTurma("abc"))
                .ReturnsAsync(new List<AulaHorario>
                {
                new AulaHorario { DiaSemana = 1, Turno = 'M', Horario = 1 }
                });

            _aulaHorarioServiceMock.Setup(s => s.ParseHorariosTurma("def"))
                .ReturnsAsync(new List<AulaHorario>
                {
                new AulaHorario { DiaSemana = 3, Turno = 'T', Horario = 5 }
                });

            // Act
            var resultado = await _turmaService.BuscarTurmasCompativeis(nomeDisciplina, preferencias);

            // Assert
            Assert.Single(resultado);
            Assert.Equal(turma1.CodigoTurmaUnico, resultado[0].CodigoTurmaUnico);
        }

        [Fact]
        public async Task BuscarTurmasCompativeis_SemTurmasDisponiveis_DeveRetornarListaVazia()
        {
            _turmaRepositoryMock.Setup(r => r.TurmasDisponiveisPorDisciplina(It.IsAny<string>()))
                .ReturnsAsync(new List<Turma>());

            var resultado = await _turmaService.BuscarTurmasCompativeis("Algoritmos", new List<AulaHorario>());

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task BuscarTurmasCompativeis_TodasIncompativeis_DeveRetornarVazio()
        {
            var turma = new Turma
            {
                CodigoTurmaUnico = 1,
                NomeDisciplina = "Algoritmos",
                Horario = "incompativel"
            };

            _turmaRepositoryMock.Setup(r => r.TurmasDisponiveisPorDisciplina(It.IsAny<string>()))
                .ReturnsAsync(new List<Turma> { turma });

            _aulaHorarioServiceMock.Setup(s => s.ParseHorariosTurma(It.IsAny<string>()))
                .ReturnsAsync(new List<AulaHorario>
                {
                new AulaHorario { DiaSemana = 5, Turno = 'N', Horario = 10 } // Não compatível
                });

            var preferencias = new List<AulaHorario>
        {
            new AulaHorario { DiaSemana = 2, Turno = 'M', Horario = 1 }
        };

            var resultado = await _turmaService.BuscarTurmasCompativeis("Algoritmos", preferencias);

            Assert.Empty(resultado);
        }
    }
}