using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface IAlunoRepository : IRepository<Aluno, string>
    {
        Task<string> ObterIdentityUserIdPorMatricula(string matricula);
        Task<bool> AlunoJaAnexouHistorico(string matricula);
        Task<(bool sucesso, string mensagem)> MarcarHistoricoComoAnexado(string matricula);
        Task<bool> AlterarNome(string matricula, string NovoNome);
        Task<bool> AdicionarCurriculoAluno(string matricula, string curriculo);
    }
}
