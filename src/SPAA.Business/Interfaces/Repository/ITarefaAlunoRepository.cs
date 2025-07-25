﻿using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface ITarefaAlunoRepository : IRepository<TarefaAluno, int>
    {
        Task<List<TarefaAluno>> TodasTarefasDoAluno(string matricula);
        Task<int?> IdTarefa(string horario, string matricula);
    }
}
