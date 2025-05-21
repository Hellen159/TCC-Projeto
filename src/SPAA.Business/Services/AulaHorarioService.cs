using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;

namespace SPAA.Business.Services
{
    public class AulaHorarioService : IAulaHorarioService
    {
        public Task<List<AulaHorario>> ParseHorariosTurma(string horarioTurma)
        {
            var horarios = new List<AulaHorario>();

            if (string.IsNullOrWhiteSpace(horarioTurma))
                return Task.FromResult(horarios);

            var blocos = horarioTurma
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim()); // <- adiciona isso para remover espaços extras

            foreach (var bloco in blocos)
            {
                // Exemplo: 46T6
                var dias = bloco.TakeWhile(char.IsDigit)
                                .Select(d => int.Parse(d.ToString()))
                                .ToList();

                var turno = bloco.FirstOrDefault(char.IsLetter);
                if (turno == default) continue;

                var horariosStr = bloco.Substring(bloco.IndexOf(turno) + 1);

                foreach (var dia in dias)
                {
                    foreach (var h in horariosStr)
                    {
                        if (!char.IsDigit(h)) continue;

                        horarios.Add(new AulaHorario
                        {
                            DiaSemana = dia,
                            Turno = turno,
                            Horario = int.Parse(h.ToString())
                        });
                    }
                }
            }

            return Task.FromResult(horarios);
        }
    }
}
