using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Services
{
    public interface IAlunoService
    {
        Task<bool> AdicionarCurriculoAluno(string matricula, string curriculo);
        Task<bool> AlterarNome(string matricula, string novoNome);
        Task<(bool sucesso, string mensagem)> MarcarHistoricoComoAnexado(string matricula);
        Task<bool> AlunoJaAnexouHistorico(string matricula);

    }
}
