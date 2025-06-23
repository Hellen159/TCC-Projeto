using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly ICursoRepository _cursoRepository;

        public AlunoService(IAlunoRepository alunoRepository,
                            ICursoRepository cursoRepository)
        {
            _alunoRepository = alunoRepository;
            _cursoRepository = cursoRepository;
        }
        public async Task<bool> AdicionarCurriculoAluno(string matricula, string curriculo)
        {
            var aluno = await _alunoRepository.ObterPorId(matricula);

            if (aluno == null)
                return (false);

            aluno.CurriculoAluno = curriculo;
            await _alunoRepository.Atualizar(aluno);

            return (true);
        }

        public async Task<bool> AlterarNome(string matricula, string novoNome)
        {
            var aluno = await _alunoRepository.ObterPorId(matricula);

            if (aluno == null)
                return (false);

            aluno.NomeAluno = novoNome;
            await _alunoRepository.Atualizar(aluno);

            return (true);
        }

        public async Task<bool> AlunoJaAnexouHistorico(string matricula)
        {
            var entidade = await _alunoRepository.ObterPorId(matricula);

            if (entidade == null)
            {
                return false; // Se o aluno não existe, ele não anexou histórico.
            }

            if (entidade.HistoricoAnexado == false)
            {
                return false;
            }

            return true;
        }

        public async Task<(bool sucesso, string mensagem)> MarcarHistoricoComoAnexado(string matricula)
        {
            var aluno = await _alunoRepository.ObterPorId(matricula);

            if (aluno == null)
                return (false, $"Aluno com matrícula {matricula} não encontrado.");

            if (aluno.HistoricoAnexado)
                return (true, "O histórico foi atualizado.");

            aluno.HistoricoAnexado = true;
            await _alunoRepository.Atualizar(aluno);

            return (true, "Histórico processado com sucesso!");
        }

        public async Task<double> PorcentagemCurso(string matricula)
        {
            var aluno = await _alunoRepository.ObterPorId(matricula);
            var curso = await _cursoRepository.ObterPorId(2);

            if (aluno == null || curso == null)
            {
                return 0;
            }

            var cargaHorariaTotalExigida = curso.CargaHorariaOptativa + curso.CargaHorariaObrigatoria;

            if (cargaHorariaTotalExigida == 0)
            {
                return 0; 
            }
            var cargaHorariaPendente = aluno.HorasObrigatoriasPendentes + aluno.HorasOptativasPendentes;

            var cargaHorariaIntegralizada = cargaHorariaTotalExigida - cargaHorariaPendente;

            return  ((double)cargaHorariaIntegralizada / cargaHorariaTotalExigida) * 100.0;
        }

        public async Task<bool> SalvarHorasAluno (string matricula, int optativas, int obrigatoria)
        {
            var aluno = await _alunoRepository.ObterPorId(matricula);

            if (aluno == null)
                return (false);

            aluno.HorasOptativasPendentes = optativas;
            aluno.HorasObrigatoriasPendentes = obrigatoria;
            await _alunoRepository.Atualizar(aluno);

            return (true);
        }
    }
}
