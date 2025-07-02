using Moq;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using System.Collections.Generic;
using System.Linq; // Adicionar para usar .Any() ou .Empty()
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

        // --- Testes para ObterDisciplinasLiberadas (já existentes e ok) ---
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

        // --- Novos testes para VerificaSeCumprePreRequisitos ---

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveRetornarTurmasSemPreRequisitos()
        {
            // Arrange
            string matricula = "AlunoSemPreReqs";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "Calculo I", CodigoTurma = "C1A" },
                new Turma { NomeDisciplina = "Introducao a Programacao", CodigoTurma = "IPA" }
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Matematica Basica" }); // Alguma disciplina aprovada, mas não relevante para pré-reqs

            // Nenhuma pré-requisito para Calculo I ou Introdução a Programação no repositório de pré-requisitos
            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "OutraDisciplina", PreRequisitoLogico = "OutroPreReq" }
                }); // Pré-requisitos para outras disciplinas, não para as turmas de teste

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains("Calculo I", resultado);
            Assert.Contains("Introducao a Programacao", resultado);
        }

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveValidarPreRequisitosParaTurmas()
        {
            // Arrange
            string matricula = "AlunoComPreReqs";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "Calculo II", CodigoTurma = "C2B" },
                new Turma { NomeDisciplina = "Estrutura de Dados", CodigoTurma = "EDA" }
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Calculo I", "Logica de Programacao" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Calculo II", PreRequisitoLogico = "Calculo I" },
                    new PreRequisito { NomeDisciplina = "Estrutura de Dados", PreRequisitoLogico = "Logica de Programacao" }
                });

            // Mockando que o serviço de pré-requisitos atende ambos
            _preRequisitoServiceMock.Setup(s => s.AtendeRequisitos("Calculo I", It.IsAny<List<string>>()))
                .ReturnsAsync(true);
            _preRequisitoServiceMock.Setup(s => s.AtendeRequisitos("Logica de Programacao", It.IsAny<List<string>>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains("Calculo II", resultado);
            Assert.Contains("Estrutura de Dados", resultado);

            // Verificar que o método AtendeRequisitos foi chamado para cada pré-requisito lógico
            _preRequisitoServiceMock.Verify(s => s.AtendeRequisitos("Calculo I", It.IsAny<List<string>>()), Times.Once);
            _preRequisitoServiceMock.Verify(s => s.AtendeRequisitos("Logica de Programacao", It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveIgnorarTurmasSeNaoAtenderRequisitos()
        {
            // Arrange
            string matricula = "AlunoNaoAtendePreReqs";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "Calculo III", CodigoTurma = "C3A" }
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Algebra Linear" }); // Não tem o pré-requisito para Calculo III

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Calculo III", PreRequisitoLogico = "Calculo II" }
                });

            // Mockando que o serviço de pré-requisitos NÃO atende
            _preRequisitoServiceMock.Setup(s => s.AtendeRequisitos("Calculo II", It.IsAny<List<string>>()))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveLidarComListaDeTurmasVazia()
        {
            // Arrange
            string matricula = "AlunoQualquer";
            var turmas = new List<Turma>(); // Lista de turmas vazia

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, It.IsAny<string>()))
                .ReturnsAsync(new List<string>()); // Não importa o que retorne, pois não será usado

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>()); // Não importa o que retorne

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            Assert.Empty(resultado);
            // Verificar que ObterNomeDisciplinasPorSituacao foi chamado (pelo menos uma vez, pode ser duas)
            _alunoDisciplinaRepoMock.Verify(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"), Times.Once);
            _alunoDisciplinaRepoMock.Verify(r => r.ObterNomeDisciplinasPorSituacao(matricula, "PEND"), Times.Never); // Nomes pendentes vem da lista de turmas.
        }

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveLidarComTurmasComNomeDisciplinaNuloOuVazio()
        {
            // Arrange
            string matricula = "AlunoComTurmasInvalidas";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "DisciplinaValida", CodigoTurma = "DVA" },
                new Turma { NomeDisciplina = null, CodigoTurma = "DNB" }, // Nome nulo
                new Turma { NomeDisciplina = "", CodigoTurma = "DVC" }, // Nome vazio
                new Turma { NomeDisciplina = " ", CodigoTurma = "DVD" } // Nome whitespace
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "PreRequisitoValido" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "DisciplinaValida", PreRequisitoLogico = "PreRequisitoValido" }
                });

            _preRequisitoServiceMock.Setup(s => s.AtendeRequisitos("PreRequisitoValido", It.IsAny<List<string>>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            // Apenas "DisciplinaValida" deve ser retornada, pois as outras têm NomeDisciplina inválido.
            // O serviço extrai nomes de disciplina da lista de turmas. Se o nome for nulo/vazio,
            // FirstOrDefault(p => p.NomeDisciplina.Equals(pendente)) para prereq será nulo,
            // e como prereq.PreRequisitoLogico será nulo/vazio, elas serão adicionadas.
            // Isso levanta um ponto: o método adiciona se não há pré-requisito lógico.
            // O comportamento atual do serviço é que, se NomeDisciplina for nulo/vazio,
            // ele não encontrará um pré-requisito, então ele adicionará à lista liberada.
            // Se esse não for o comportamento desejado, o serviço precisaria de validação extra.
            // Para este teste, esperamos que o serviço se comporte como está implementado.
            Assert.Equal(4, resultado.Count); // Todos são adicionados se não tem pré-req ou se o pré-req é cumprido.
            Assert.Contains("DisciplinaValida", resultado);
            Assert.Contains(null, resultado); // Nome nulo
            Assert.Contains("", resultado);   // Nome vazio
            Assert.Contains(" ", resultado);  // Nome whitespace

            // Se o comportamento desejado fosse NÃO adicionar nulos/vazios, a lógica no serviço precisaria de um 'where' extra:
            // foreach (var pendente in nomesPendentes.Where(n => !string.IsNullOrWhiteSpace(n)))
        }


        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveLidarComNenhumPreRequisitoRegistrado()
        {
            // Arrange
            string matricula = "AlunoSemPreReqRegistrados";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "Historia", CodigoTurma = "HIS1" },
                new Turma { NomeDisciplina = "Geografia", CodigoTurma = "GEO1" }
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string> { "Filosofia" });

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>()); // Repositório de pré-requisitos retorna vazio

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            // Se não há pré-requisitos registrados, todas as disciplinas são consideradas liberadas.
            Assert.Equal(2, resultado.Count);
            Assert.Contains("Historia", resultado);
            Assert.Contains("Geografia", resultado);
            _preRequisitoServiceMock.Verify(s => s.AtendeRequisitos(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never); // AtendeRequisitos não deve ser chamado
        }

        [Fact]
        public async Task VerificaSeCumprePreRequisitos_DeveLidarComDisciplinasAprovadasVazias()
        {
            // Arrange
            string matricula = "AlunoSemAprovadas";
            var turmas = new List<Turma>
            {
                new Turma { NomeDisciplina = "Logica I", CodigoTurma = "L1" },
                new Turma { NomeDisciplina = "Logica II", CodigoTurma = "L2" }
            };

            _alunoDisciplinaRepoMock.Setup(r => r.ObterNomeDisciplinasPorSituacao(matricula, "APR"))
                .ReturnsAsync(new List<string>()); // Nenhuma disciplina aprovada

            _preRequisitoRepoMock.Setup(r => r.ObterTodos())
                .ReturnsAsync(new List<PreRequisito>
                {
                    new PreRequisito { NomeDisciplina = "Logica I", PreRequisitoLogico = "" }, // Sem pré-requisito
                    new PreRequisito { NomeDisciplina = "Logica II", PreRequisitoLogico = "Logica I" } // Com pré-requisito não atendido
                });

            _preRequisitoServiceMock.Setup(s => s.AtendeRequisitos("Logica I", It.IsAny<List<string>>()))
                .ReturnsAsync(false); // Não atende, pois a lista de aprovadas é vazia

            // Act
            var resultado = await _service.VerificaSeCumprePreRequisitos(turmas, matricula);

            // Assert
            // Apenas "Logica I" deve ser liberada (por não ter pré-requisito lógico)
            Assert.Single(resultado);
            Assert.Contains("Logica I", resultado);
            Assert.DoesNotContain("Logica II", resultado);
        }
    }
}