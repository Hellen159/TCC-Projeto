using Microsoft.EntityFrameworkCore;
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

        public async Task<int?> IdTarefa(string horario, string matricula)
        {
            var tarefa = await DbSet
                               .Where(ta => ta.Matricula == matricula && ta.Horario == horario)
                               .Select(ta => ta.CodigoTarefaAluno) 
                               .FirstOrDefaultAsync();

            return tarefa;
        }

        public async Task<List<TarefaAluno>> TodasTarefasDoAluno(string matricula)
        {
            var tarefas = new List<TarefaAluno>();
            tarefas = await DbSet
                 .Where(ta => ta.Matricula == matricula)
                 .ToListAsync();

            if (!tarefas.Any())
                return tarefas;

            return tarefas;
        }
    }
}
