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

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
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
    }
}
