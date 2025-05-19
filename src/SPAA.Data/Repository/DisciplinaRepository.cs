using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System.Text.RegularExpressions;

namespace SPAA.Data.Repository
{
    public class DisciplinaRepository : Repository<Disciplina, string>, IDisciplinaRepository
    {
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        private readonly IPreRequisitoRepository _preRequisitoRepository;

        public DisciplinaRepository(MeuDbContext context, 
                                    IAlunoDisciplinaRepository alunoDisciplinaRepository, 
                                    IPreRequisitoRepository preRequisitoRepository) : base(context)
        {
            _alunoDisciplinaRepository = alunoDisciplinaRepository;
            _preRequisitoRepository = preRequisitoRepository;
        }

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

                if (await _preRequisitoRepository.AtendeRequisitos(prereq.PreRequisitoLogico, nomesAprovadas))
                {
                    disciplinasLiberadas.Add(pendente);
                }
            }

            return disciplinasLiberadas;
        }
    }
}
