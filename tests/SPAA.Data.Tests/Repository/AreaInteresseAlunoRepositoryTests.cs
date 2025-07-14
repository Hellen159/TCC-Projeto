//using Microsoft.EntityFrameworkCore;
//using SPAA.Business.Models;
//using SPAA.Data.Context;
//using SPAA.Data.Repository;
//using System;
//using System.Threading.Tasks;
//using Xunit;

//namespace SPAA.Data.Tests.Repository
//{
//    public class AreaInteresseAlunoRepositoryTests
//    {
//        private MeuDbContext ObterContextoEmMemoria()
//        {
//            var options = new DbContextOptionsBuilder<MeuDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            return new MeuDbContext(options);
//        }

//        [Fact]
//        public async Task AlunoJaTemAreaInteresse_DeveRetornarTrue_QuandoAreaInteresseExistirParaMatricula()
//        {
//            // Arrange
//            var contexto = ObterContextoEmMemoria();
//            var areaInteresse = new AreaInteresseAluno
//            {
//                CodigoAreaInteresseAluno = 1,
//                Matricula = "123456789",
//                NivelInteresse = "primario" // exemplo, coloque o valor correto conforme seu modelo
//            };


//            contexto.Set<AreaInteresseAluno>().Add(areaInteresse);
//            await contexto.SaveChangesAsync();

//            var repo = new AreaInteresseAlunoRepository(contexto);

//            // Act
//            var existe = await repo.AlunoJaTemAreaInteresse("123456789");

//            // Assert
//            Assert.True(existe);
//        }

//        [Fact]
//        public async Task AlunoJaTemAreaInteresse_DeveRetornarFalse_QuandoNaoExistirAreaInteresseParaMatricula()
//        {
//            // Arrange
//            var contexto = ObterContextoEmMemoria();
//            var repo = new AreaInteresseAlunoRepository(contexto);

//            // Act
//            var existe = await repo.AlunoJaTemAreaInteresse("matricula-inexistente");

//            // Assert
//            Assert.False(existe);
//        }
//    }
//}