using SPAA.Business.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Business.Tests.Services
{
    public class PreRequisitoServiceTests
    {
        private readonly PreRequisitoService _service = new PreRequisitoService();

        [Fact]
        public async Task AtendeRequisitos_ExpressaoVazia_DeveRetornarTrue()
        {
            var resultado = await _service.AtendeRequisitos("", new List<string>());
            Assert.True(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_UmaDisciplinaAprovada_DeveRetornarTrue()
        {
            var resultado = await _service.AtendeRequisitos("Algoritmos", new List<string> { "Algoritmos" });
            Assert.True(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_UmaDisciplinaNaoAprovada_DeveRetornarFalse()
        {
            var resultado = await _service.AtendeRequisitos("Estrutura de Dados", new List<string> { "Algoritmos" });
            Assert.False(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_ComEOperadorTodasAprovadas_DeveRetornarTrue()
        {
            var resultado = await _service.AtendeRequisitos("Algoritmos && Estrutura de Dados",
                new List<string> { "Algoritmos", "Estrutura de Dados" });
            Assert.True(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_ComEOperadorParcialAprovadas_DeveRetornarFalse()
        {
            var resultado = await _service.AtendeRequisitos("Algoritmos && Estrutura de Dados",
                new List<string> { "Algoritmos" });
            Assert.False(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_ComOuOperadorUmaAprovada_DeveRetornarTrue()
        {
            var resultado = await _service.AtendeRequisitos("Algoritmos || Cálculo I",
                new List<string> { "Cálculo I" });
            Assert.True(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_ExpressaoMalFormada_DeveRetornarFalse()
        {
            var resultado = await _service.AtendeRequisitos("Algoritmos &&",
                new List<string> { "Algoritmos" });
            Assert.False(resultado);
        }

        [Fact]
        public async Task AtendeRequisitos_ComParentesesELogicaCorreta_DeveRetornarTrue()
        {
            var resultado = await _service.AtendeRequisitos("(Algoritmos && Estrutura de Dados) || Cálculo I",
                new List<string> { "Cálculo I" });
            Assert.True(resultado);
        }
    }
}