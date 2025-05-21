using Microsoft.AspNetCore.Http;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Services
{
    public interface IAlunoDisciplinaService
    {
        Task<string> ExtrairTextoDePdf(IFormFile arquivoPdf);
        Task<List<string>> ExtrairBlocos(string texto);
        Task<List<AlunoDisciplina>> ObterEquivalenciasCurriculo(string texto, string matricula);
        Task<List<AlunoDisciplina>> ConverterBlocos(List<string> blocos, string matricula);
        Task<List<AlunoDisciplina>> ObterObrigatoriasPendentes(string texto, string matricula);
        Task <string> ObterInformacoesCurriculo(string texto);
    }
}
