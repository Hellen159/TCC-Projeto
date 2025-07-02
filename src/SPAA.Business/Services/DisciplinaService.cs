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
    public class DisciplinaService : IDisciplinaService
    {
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IPreRequisitoService _preRequisitoService;
        private readonly IPreRequisitoRepository _preRequisitoRepository;

        public DisciplinaService(IAlunoDisciplinaRepository alunoDisciplinaRepository,
                                 IPreRequisitoService preRequisitoService,
                                 IPreRequisitoRepository preRequisitoRepository)
        {
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _preRequisitoService = preRequisitoService;
            _preRequisitoRepository = preRequisitoRepository;
        }

        //busca disciplinas que tem todos os pre requisitos satisfeitos
        public async Task<List<string>> ObterDisciplinasLiberadas(string matricula)
        {
            var nomesAprovadas = await _alunoDisciplinaRepository.ObterNomeDisciplinasPorSituacao(matricula, "APR");
            var nomesPendentes = await _alunoDisciplinaRepository.ObterNomeDisciplinasPorSituacao(matricula, "PEND");
            var preRequisitos = await _preRequisitoRepository.ObterTodos();
            var disciplinasLiberadas = new List<string>();

            foreach (var pendente in nomesPendentes)
            {
                var prereq = preRequisitos.FirstOrDefault(p =>
                    p.NomeDisciplina.Equals(pendente, StringComparison.InvariantCultureIgnoreCase));

                if (prereq == null || string.IsNullOrWhiteSpace(prereq.PreRequisitoLogico))
                {
                    disciplinasLiberadas.Add(pendente);
                    continue;
                }

                if (await _preRequisitoService.AtendeRequisitos(prereq.PreRequisitoLogico, nomesAprovadas))
                {
                    disciplinasLiberadas.Add(pendente);
                }
            }

            return disciplinasLiberadas;
        }

        //busca disciplinas que tem todos os pre requisitos satisfeitos
        public async Task<List<string>> VerificaSeCumprePreRequisitos(List<Turma>turmas, string matricula)
        {
            var nomesAprovadas = await _alunoDisciplinaRepository.ObterNomeDisciplinasPorSituacao(matricula, "APR");
            var nomesPendentes = new List<string>();
            
            foreach (var turma in turmas)
            {
                nomesPendentes.Add(turma.NomeDisciplina);
            }

            var preRequisitos = await _preRequisitoRepository.ObterTodos();
            var disciplinasLiberadas = new List<string>();

            foreach (var pendente in nomesPendentes)
            {
                var prereq = preRequisitos.FirstOrDefault(p =>
                    p.NomeDisciplina.Equals(pendente, StringComparison.InvariantCultureIgnoreCase));

                if (prereq == null || string.IsNullOrWhiteSpace(prereq.PreRequisitoLogico))
                {
                    disciplinasLiberadas.Add(pendente);
                    continue;
                }

                if (await _preRequisitoService.AtendeRequisitos(prereq.PreRequisitoLogico, nomesAprovadas))
                {
                    disciplinasLiberadas.Add(pendente);
                }
            }

            return disciplinasLiberadas;
        }
    }
}
