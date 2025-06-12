using Microsoft.EntityFrameworkCore;
using SPAA.Business.Models;
using SPAA.Data.Context;
using SPAA.Data.Repository;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Data.Tests.Repository
{
    // Repositório concreto para Disciplina
    public class DisciplinaRepository : Repository<Disciplina, int>
    {
        public DisciplinaRepository(MeuDbContext context) : base(context) { }
    }

    public class DisciplinaRepositoryTests : IDisposable
    {
        private readonly MeuDbContext _context;
        private readonly DisciplinaRepository _repo;

        public DisciplinaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MeuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MeuDbContext(options);
            _repo = new DisciplinaRepository(_context);
        }

        [Fact]
        public async Task Adicionar_DeveAdicionarDisciplina()
        {
            var disciplina = new Disciplina
            {
                NomeDisciplina = "Matemática",
                CargaHoraria = 60,
                Codigo = "MAT101"
            };

            var resultado = await _repo.Adicionar(disciplina);

            Assert.True(resultado);
            Assert.NotEqual(0, disciplina.CodigoDisciplina); // EF deve gerar o Id
            var disciplinaNoBanco = await _repo.ObterPorId(disciplina.CodigoDisciplina);
            Assert.NotNull(disciplinaNoBanco);
            Assert.Equal("Matemática", disciplinaNoBanco.NomeDisciplina);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarDisciplinaExistente()
        {
            var disciplina = new Disciplina
            {
                NomeDisciplina = "Física",
                CargaHoraria = 50,
                Codigo = "FIS101"
            };
            await _repo.Adicionar(disciplina);

            var obtida = await _repo.ObterPorId(disciplina.CodigoDisciplina);

            Assert.NotNull(obtida);
            Assert.Equal("Física", obtida.NomeDisciplina);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNull_QuandoNaoExistir()
        {
            var obtida = await _repo.ObterPorId(9999);
            Assert.Null(obtida);
        }

        [Fact]
        public async Task Remover_DeveRemoverDisciplinaExistente()
        {
            var disciplina = new Disciplina
            {
                NomeDisciplina = "Química",
                CargaHoraria = 40,
                Codigo = "QUI101"
            };
            await _repo.Adicionar(disciplina);

            var removido = await _repo.Remover(disciplina.CodigoDisciplina);

            Assert.True(removido);
            var obtida = await _repo.ObterPorId(disciplina.CodigoDisciplina);
            Assert.Null(obtida);
        }

        [Fact]
        public async Task Remover_DeveRetornarFalse_QuandoDisciplinaNaoExistir()
        {
            var removido = await _repo.Remover(9999);
            Assert.False(removido);
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarDisciplinaExistente()
        {
            var disciplina = new Disciplina
            {
                NomeDisciplina = "Biologia",
                CargaHoraria = 45,
                Codigo = "BIO101"
            };
            await _repo.Adicionar(disciplina);

            disciplina.NomeDisciplina = "Biologia Atualizada";
            await _repo.Atualizar(disciplina);

            var atualizada = await _repo.ObterPorId(disciplina.CodigoDisciplina);
            Assert.Equal("Biologia Atualizada", atualizada.NomeDisciplina);
        }

        [Fact]
        public async Task Atualizar_DeveLancarExcecao_QuandoEntidadeNula()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.Atualizar(null));
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarTodasDisciplinas()
        {
            await _repo.Adicionar(new Disciplina { NomeDisciplina = "A", CargaHoraria = 10, Codigo = "A1" });
            await _repo.Adicionar(new Disciplina { NomeDisciplina = "B", CargaHoraria = 20, Codigo = "B1" });

            var todas = await _repo.ObterTodos();

            Assert.NotNull(todas);
            Assert.True(todas.Count >= 2);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
