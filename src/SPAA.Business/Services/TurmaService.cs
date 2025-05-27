using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Services
{
    public class TurmaService : ITurmaService
    {
        private readonly IAulaHorarioService _aulaHorarioService;
        private readonly ITurmaRepository _turmaRepository;

        public TurmaService(IAulaHorarioService aulaHorarioService,
                            ITurmaRepository turmaRepository)
        {
            _aulaHorarioService = aulaHorarioService;
            _turmaRepository = turmaRepository;
        }

        public async Task<List<Turma>> BuscarTurmasCompativeis(string nomeDisciplina, List<AulaHorario> preferencias)
        {
            var turmas = await _turmaRepository.TurmasDisponiveisPorDisciplina(nomeDisciplina);

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
    }
}
