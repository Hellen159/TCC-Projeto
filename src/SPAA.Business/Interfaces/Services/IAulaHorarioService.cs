﻿using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Services
{
    public interface IAulaHorarioService
    {
        Task<List<AulaHorario>> ParseHorariosTurma(string horarioTurma);
    }
}
