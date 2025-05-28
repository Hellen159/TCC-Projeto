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
    public class DisciplinaServiceTests
    {
        private readonly Mock<IAlunoDisciplinaRepository> _alunoDisciplinaRepoMock;
        private readonly Mock<IPreRequisitoService> _preRequisitoServiceMock;
        private readonly Mock<IPreRequisitoRepository> _preRequisitoRepoMock;
        private readonly DisciplinaService _service;

        public DisciplinaServiceTests()
        {
            _alunoDisciplinaRepoMock = new Mock<IAlunoDisciplinaRepository>();
            _preRequisitoServiceMock = new Mock<IPreRequisitoService>();
            _preRequisitoRepoMock = new Mock<IPreRequisitoRepository>();

            _service = new DisciplinaService(
                _alunoDisciplinaRepoMock.Object,
                _preRequisitoServiceMock.Object,
                _preRequisitoRepoMock.Object
            );
        }

        [Fact]
        public async Task ObterDisciplinasLiberadas_DeveRetornarDisciplinasSemPreRequisitos()
        {
            // Arrange
            string matricula = "12345";

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Matematica" });

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "PEND"))
                .ReturnsAsync(new List<string> { "Fisica" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Fisica", PreRequisitoLogico = "" }
                });

            // Act
            var resultado = await _service.ObterDisciplinasLiberadas(matricula);

            // Assert
            Assert.Single(resultado);
            Assert.Contains("Fisica", resultado);
        }

        [Fact]
        public async Task ObterDisciplinasLiberadas_DeveValidarPreRequisitos()
        {
            // Arrange
            string matricula = "67890";

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Matematica" });

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "PEND"))
                .ReturnsAsync(new List<string> { "Fisica" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Fisica", PreRequisitoLogico = "Matematica" }
                });

            _preRequisitoServiceMock.Setup(s =>
                s.AtendeRequisitos("Matematica", It.IsAny<List<string>>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.ObterDisciplinasLiberadas(matricula);

            // Assert
            Assert.Single(resultado);
            Assert.Contains("Fisica", resultado);
        }

        [Fact]
        public async Task ObterDisciplinasLiberadas_DeveIgnorarSeNaoAtenderRequisitos()
        {
            string matricula = "99999";

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "História" });

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "PEND"))
                .ReturnsAsync(new List<string> { "Fisica" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Fisica", PreRequisitoLogico = "Matematica" }
                });

            _preRequisitoServiceMock.Setup(s =>
                s.AtendeRequisitos("Matematica", It.IsAny<List<string>>()))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.ObterDisciplinasLiberadas(matricula);

            // Assert
            Assert.Empty(resultado);
        }
    }
}
