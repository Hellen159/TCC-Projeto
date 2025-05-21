using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class PreRequisitoRepository : Repository<PreRequisito, int>, IPreRequisitoRepository
    {
        public PreRequisitoRepository(MeuDbContext context) : base(context)
        {
        }

    }
}
