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
    public class TurmaRepository : Repository<Turma, string>, ITurmaRepository
    {
        public TurmaRepository(MeuDbContext context) : base(context)
        {
        }

    }
}
