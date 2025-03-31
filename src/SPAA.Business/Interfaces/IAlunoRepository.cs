using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IAlunoRepository 
    {
        Task CriarAluno(Aluno aluno);
    }
}
