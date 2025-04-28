using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class AlunoRepository : Repository<Aluno, string>, IAlunoRepository
    {
        public AlunoRepository(MeuDbContext context) : base(context) { }

        public async Task<bool> AlterarNome(string matricula, string NovoNome)
        {
            var aluno = await ObterPorId(matricula);

            if (aluno == null)
                return (false);

            aluno.NomeAluno = NovoNome;
            await Atualizar(aluno);

            return (true);
        }

        public async Task<bool> AlunoJaAnexouHistorico(string matricula)
        {

            var entidade = await ObterPorId(matricula);

            if (entidade.HistoricoAnexado == false)
            {
                return false;
            }

            return true;
        }

        public async Task<(bool sucesso, string mensagem)> MarcarHistoricoComoAnexado(string matricula)
        {
            var aluno = await ObterPorId(matricula); 

            if (aluno == null)
                return (false, $"Aluno com matrícula {matricula} não encontrado.");

            if (aluno.HistoricoAnexado)
                return (true, "O histórico foi atualizado.");

            aluno.HistoricoAnexado = true;
            await Atualizar(aluno);

            return (true, "Histórico processado com sucesso!");
        }

        public async Task<string> ObterIdentityUserIdPorMatricula(string matricula)
        {
            var entidade = await ObterPorId(matricula);

            if (entidade is Aluno aluno)
            {
                return aluno.User.Id;
            }

            throw new Exception("Entidade não encontrada ou não é um Aluno.");
        }
    }
}
