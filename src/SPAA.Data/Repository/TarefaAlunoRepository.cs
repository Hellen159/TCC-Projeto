using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class TarefaAlunoRepository : Repository<TarefaAluno, int>, ITarefaAlunoRepository
    {
        public TarefaAlunoRepository(MeuDbContext context) : base(context)
        {
        }
    }
}
