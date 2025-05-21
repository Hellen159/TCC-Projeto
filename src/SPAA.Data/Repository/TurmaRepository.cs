using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class TurmaRepository : Repository<Turma, int>, ITurmaRepository
    {
        private readonly IAulaHorarioService _aulaHorarioService;
        public TurmaRepository(MeuDbContext context, 
                                IAulaHorarioService aulaHorarioService) : base(context)
        {
            _aulaHorarioService = aulaHorarioService;
        }

        public async Task<List<Turma>> BuscarTurmasCompativeis(string nomeDisciplina, List<AulaHorario> preferencias)
        {
            var turmas = await TurmasDisponiveisPorDisciplina(nomeDisciplina);

            var turmasCompativeis = new List<Turma>();

            foreach (var turma in turmas)
            {
                var horariosTurma = await _aulaHorarioService.ParseHorariosTurma(turma.Horario);

                bool compativel = horariosTurma.All(h =>
                    preferencias.Any(p =>
                        p.DiaSemana == h.DiaSemana &&
                        p.Turno == h.Turno &&
                        p.Horario == h.Horario
                    )
                );

                if (compativel)
                    turmasCompativeis.Add(turma);
            }

            return turmasCompativeis;
        }

        public async Task<List<Turma>> TurmasDisponiveisPorDisciplina(string nomeMateria)
        {
            return await  DbSet.Where(t => t.NomeDisciplina == nomeMateria).ToListAsync();
        }
    }
}
