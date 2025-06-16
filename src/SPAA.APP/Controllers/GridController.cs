using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.ViewModels;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System.Text.Json;
using System.Linq; // Necessário para .Any() e outros métodos LINQ

namespace SPAA.App.Controllers
{
    public class GridController : Controller
    {
        private readonly IDisciplinaService _disciplinaService;
        private readonly ITurmaService _turmaService;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ICurriculoRepository _curriculoRepository;
        private readonly IMapper _mapper;
        private readonly ITurmaSalvaRepository _turmaSalvaRepository;

        public GridController(IDisciplinaService disciplinaService,
                               ITurmaService turmaService,
                               ITurmaRepository turmaRepository,
                               IAlunoRepository alunoRepository,
                               ICurriculoRepository curriculoRepository,
                               IMapper mapper,
                               ITurmaSalvaRepository turmaSalvaRepository)
        {
            _disciplinaService = disciplinaService;
            _turmaService = turmaService;
            _turmaRepository = turmaRepository;
            _alunoRepository = alunoRepository;
            _curriculoRepository = curriculoRepository;
            _mapper = mapper;
            _turmaSalvaRepository = turmaSalvaRepository;
        }

        // --- Método GET: Carrega a página inicialmente com turmas obrigatórias e optativas ---
        [HttpGet]
        public async Task<IActionResult> MontarGrade()
        {
            var resultado = new MontarGradeResultViewModel
            {
                Turmas = new List<TurmaViewModel>(),         // Inicializa
                TurmasOptativas = new List<TurmaViewModel>() // Inicializa
            };

            try
            {
                // Chama o método privado para carregar as turmas obrigatórias
                var turmasObrigatorias = await _CarregarTurmasObrigatorias();
                resultado.Turmas = turmasObrigatorias.Item1; // Item1 é a lista de TurmaViewModel
                // Item2 é a mensagem, mas aqui não precisamos dela a menos que queira tratar no frontend

                // Chama o método privado para carregar as turmas optativas
                var turmasOptativas = await _CarregarTurmasOptativas();
                resultado.TurmasOptativas = turmasOptativas.Item1; // Item1 é a lista de TurmaViewModel

                // Carregar turmas salvas aqui também
                var turmasSalvas = await _turmaSalvaRepository.TodasTurmasSalvasAluno(User.Identity.Name);
                var codigosUnicosSalvos = turmasSalvas.Select(ts => ts.CodigoUnicoTurma).ToList();
                ViewData["TurmasSalvasCodigos"] = codigosUnicosSalvos;

                Console.WriteLine($"[MontarGrade] - Codigos Unicos Salvos: {codigosUnicosSalvos}");


                // Lógica de mensagens pode precisar de ajuste dependendo de como você quer combinar as mensagens
                if (!resultado.Turmas.Any() && !resultado.TurmasOptativas.Any())
                {
                    resultado.Mensagem = "Não foram encontradas turmas obrigatórias ou optativas disponíveis no momento.";
                }
                else if (!resultado.Turmas.Any())
                {
                    resultado.Mensagem = "Parabéns! Você não possui disciplinas obrigatórias pendentes.";
                }
                else if (resultado.Turmas.Any() || resultado.TurmasOptativas.Any())
                {
                    resultado.Mensagem = "Turmas disponíveis carregadas com sucesso.";
                }
            }
            catch (Exception ex)
            {
                resultado.Mensagem = $"Erro ao carregar as turmas: {ex.Message}";
                Console.WriteLine($"Erro ao carregar as turmas (GET): {ex.Message}");
                resultado.Turmas = new List<TurmaViewModel>();
                resultado.TurmasOptativas = new List<TurmaViewModel>();
            }

            return View("~/Views/DashBoard/MontarGrade.cshtml", resultado);
        }


        // --- Método POST: Processa a disponibilidade enviada pelo usuário ---
        [HttpPost]
        public async Task<IActionResult> MontarGrade(MontarGradeViewModel model)
        {
            var resultado = new MontarGradeResultViewModel
            {
                Turmas = new List<TurmaViewModel>(),         // Sempre inicialize
                TurmasOptativas = new List<TurmaViewModel>() // Sempre inicialize
            };

            // Lógica para processar as preferências do usuário (se ModelState for válido)
            if (ModelState.IsValid)
            {
                var preferencias = new List<AulaHorario>();

                try
                {
                    var horarios = JsonSerializer.Deserialize<List<string>>(model.HorariosMarcados);

                    foreach (var horarioString in horarios)
                    {
                        string cleaned = horarioString.Replace(" ", "").ToUpper();

                        if (cleaned.Length >= 3 &&
                            char.IsDigit(cleaned[0]) && // Garante que o primeiro char é um dígito (dia da semana)
                            char.IsLetter(cleaned[1]) && // Garante que o segundo char é uma letra (turno)
                            char.IsDigit(cleaned[2]) && // Garante que o terceiro char é um dígito (horário)
                            int.TryParse(cleaned[0].ToString(), out int diaSemana) &&
                            int.TryParse(cleaned.Substring(2), out int horario))
                        {
                            preferencias.Add(new AulaHorario
                            {
                                DiaSemana = diaSemana,
                                Turno = cleaned[1],
                                Horario = horario
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Erro ao interpretar o formato do horário: '{horarioString}'");
                        }
                    }

                    var disciplinasPendentes = await _disciplinaService.ObterDisciplinasLiberadas(User.Identity.Name);

                    if (disciplinasPendentes == null || !disciplinasPendentes.Any())
                    {
                        resultado.Mensagem = "Parabéns! Você não possui disciplinas obrigatórias pendentes.";
                    }
                    else
                    {
                        var turmasCompativelComHorario = new List<Turma>();

                        foreach (var disciplina in disciplinasPendentes)
                        {
                            var turmas = await _turmaService.BuscarTurmasCompativeis(disciplina, preferencias);
                            turmasCompativelComHorario.AddRange(turmas);
                        }

                        if (!turmasCompativelComHorario.Any())
                        {
                            resultado.Mensagem = "Nenhuma turma compatível foi encontrada. Por isso, estamos exibindo todas as turmas para as quais você já possui os pré-requisitos. Selecione a turma desejada e clique em “Salvar turma”.";
                            // Se não houver turmas compatíveis, recarregar todas as obrigatórias
                            var todasTurmasDisponiveis = await _CarregarTurmasObrigatorias();
                            resultado.Turmas = todasTurmasDisponiveis.Item1;
                        }
                        else
                        {
                            resultado.Mensagem = "Turmas encontradas com sucesso. Selecione a turma desejada e clique em “Salvar turma”.";
                            resultado.Turmas = _mapper.Map<List<TurmaViewModel>>(turmasCompativelComHorario);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    resultado.Mensagem = "Erro ao interpretar os horários enviados. Por favor, tente novamente.";
                    Console.WriteLine($"Erro de JSON ao processar horários marcados (POST): {ex.Message}");
                }
                catch (Exception ex)
                {
                    resultado.Mensagem = $"Ocorreu um erro inesperado ao carregar a grade: {ex.Message}";
                    Console.WriteLine($"Erro inesperado em MontarGrade (POST - processamento): {ex.Message}");
                }
            }
            else // ModelState.IsValid é false
            {
                resultado.Mensagem = "Erro: Selecione os horários antes de enviar a disponibilidade!";
            }

            // --- Recarregar Turmas Obrigatórias e Optativas em qualquer cenário (sucesso ou falha) ---
            // Isso garante que as listas de turmas (Turmas e TurmasOptativas)
            // sejam sempre populadas antes de retornar a view.
            try
            {
                // Se a lista de turmas obrigatórias já foi preenchida no bloco acima com turmas compatíveis,
                // não a sobrescrevemos. Caso contrário, recarregamos.
                if (!resultado.Turmas.Any())
                {
                    var turmasObrigatoriasRecarregadas = await _CarregarTurmasObrigatorias();
                    resultado.Turmas = turmasObrigatoriasRecarregadas.Item1;
                }

                var turmasOptativasRecarregadas = await _CarregarTurmasOptativas();
                resultado.TurmasOptativas = turmasOptativasRecarregadas.Item1;

                // Carrega turmas salvas do aluno
                var turmasSalvas = await _turmaSalvaRepository.TodasTurmasSalvasAluno(User.Identity.Name);
                var codigosUnicosSalvos = turmasSalvas.Select(ts => ts.CodigoUnicoTurma).ToList();
                var turmasSalvasViewModel = _mapper.Map<List<TurmaViewModel>>(turmasSalvas);

                // Adiciona as turmas salvas à resposta
                // Envia os códigos para a view
                ViewData["TurmasSalvasCodigos"] = codigosUnicosSalvos;
                ViewData["TurmasSalvas"] = turmasSalvasViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao recarregar turmas no final do POST: {ex.Message}");
            }

            return View("~/Views/DashBoard/MontarGrade.cshtml", resultado);
        }

        [HttpPost]
        public async Task<IActionResult> SalvarGrade([FromBody] List<TurmaViewModel> turmasSelecionadas)
        {
            try
            {
                if (turmasSelecionadas == null || !turmasSelecionadas.Any())
                {
                    return BadRequest("Nenhuma turma foi selecionada.");
                }

                // Aqui você processa as turmas (ex: salvar no banco de dados)
                // Exemplo:
                var turmasParaSalvar = _mapper.Map<List<TurmaSalva>>(turmasSelecionadas);

                string matriculaDoAluno = User.Identity.Name;

                if (string.IsNullOrEmpty(matriculaDoAluno))
                {
                    return Unauthorized("Não foi possível identificar a matrícula do usuário.");
                }

                await _turmaSalvaRepository.ExcluirTurmasSalvasDoAluno(matriculaDoAluno);

                foreach (var turma in turmasParaSalvar)
                {
                    turma.Matricula = matriculaDoAluno;
                    await _turmaSalvaRepository.Adicionar(turma);
                }


                return Ok(new { success = true, message = "Grade salva com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar grade: {ex.Message}");
            }
        }
        private async Task<Tuple<List<TurmaViewModel>, string>> _CarregarTurmasObrigatorias()
        {
            var turmasViewModel = new List<TurmaViewModel>();
            string mensagem = string.Empty;

            try
            {
                var disciplinasPendentes = await _disciplinaService.ObterDisciplinasLiberadas(User.Identity.Name);
                var turmasBusiness = new List<Turma>();

                if (disciplinasPendentes != null && disciplinasPendentes.Any())
                {
                    foreach (var disciplina in disciplinasPendentes)
                    {
                        var turmas = await _turmaRepository.TurmasDisponiveisPorDisciplina(disciplina);
                        turmasBusiness.AddRange(turmas);
                    }
                    turmasViewModel = _mapper.Map<List<TurmaViewModel>>(turmasBusiness);
                }
                else
                {
                    mensagem = "Parabéns! Você não possui disciplinas obrigatórias pendentes.";
                }
            }
            catch (Exception ex)
            {
                mensagem = $"Erro ao carregar turmas obrigatórias: {ex.Message}";
                Console.WriteLine($"Erro em _CarregarTurmasObrigatorias: {ex.Message}");
            }
            return Tuple.Create(turmasViewModel, mensagem);
        }

        private async Task<Tuple<List<TurmaViewModel>, string>> _CarregarTurmasOptativas()
        {
            var turmasViewModel = new List<TurmaViewModel>();
            string mensagem = string.Empty;

            try
            {
                var aluno = await _alunoRepository.ObterPorId(User.Identity.Name);
                var turmasBusiness = new List<Turma>();

                if (aluno != null && aluno.CurriculoAluno != null)
                {
                    var curriculosOptativos = await _curriculoRepository.ObterDisciplinasPorCurrciulo(aluno.CurriculoAluno, 2);

                    if (curriculosOptativos != null && curriculosOptativos.Any())
                    {
                        foreach (var curriculo in curriculosOptativos)
                        {
                            var turmas = await _turmaRepository.TurmasDisponiveisPorDisciplina(curriculo.NomeDisciplina);
                            turmasBusiness.AddRange(turmas);
                        }
                        turmasViewModel = _mapper.Map<List<TurmaViewModel>>(turmasBusiness);
                    }
                    else
                    {
                        mensagem = "Não foram encontradas turmas optativas para o seu currículo.";
                    }
                }
                else
                {
                    mensagem = "Erro ao buscar turmas optativas: Currículo do aluno não encontrado ou inválido.";
                }
            }
            catch (Exception ex)
            {
                mensagem = $"Erro ao carregar turmas optativas: {ex.Message}";
                Console.WriteLine($"Erro em _CarregarTurmasOptativas: {ex.Message}");
            }
            return Tuple.Create(turmasViewModel, mensagem);
        }
    }
}