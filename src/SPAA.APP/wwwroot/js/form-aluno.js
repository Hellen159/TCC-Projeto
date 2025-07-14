let calculatedProfileData = null; // Para armazenar o perfil principal e secundários

const perguntas = [
    {
        id: "1",
        texto: "Durante o desenvolvimento de um sistema para uma organização, a equipe precisa tomar decisões críticas. Uma delas é escolher entre priorizar a estrutura robusta do backend ou focar primeiro na prototipação rápida da interface. O prazo é apertado, e ambas são importantes. Diante disso, qual decisão você defenderia com mais convicção?",
        opcoes: [
            { valor: "A", texto: "Eu faria qualquer uma das duas, desde que alguém me dissesse claramente o que fazer.", perfil: "Nada" },
            { valor: "B", texto: "Começar pela base do sistema, garantindo uma arquitetura sólida desde o início.", perfil: "Programação e Infraestrutura Técnica" },
            { valor: "C", texto: "Focar primeiro no que o usuário verá e interagirá, mesmo que depois precise refatorar.", perfil: "Projeto e Análise de Software" }
        ]
    },
    {
        id: "2",
        texto: "Uma funcionalidade lançada recentemente começou a apresentar falhas intermitentes. A equipe precisa resolver a situação rapidamente. Diante da pressão do tempo, qual seria seu primeiro passo?",
        opcoes: [
            { valor: "A", texto: "Investigar causas profundas, revisar decisões anteriores e entender o impacto no design.", perfil: "Projeto e Análise de Software" },
            { valor: "B", texto: "Eu começaria por qualquer um dos dois caminhos, dependendo de quem me pedisse para agir primeiro.", perfil: "Nada" },
            { valor: "C", texto: "Aplicar testes automatizados, revisar logs e garantir estabilidade antes de investigar causas profundas.", perfil: "Qualidade, Testes e Manutenção" }
        ]
    },
    {
        id: "3",
        texto: "Um sistema antigo precisa ser mantido funcionando, mas a documentação está desatualizada e há dívidas técnicas. A equipe deve dividir as tarefas urgentemente. Qual papel você assumiria nessa situação?",
        opcoes: [
            { valor: "A", texto: "Atualizar a documentação, entender o funcionamento e propor melhorias no processo.", perfil: "Gestão de Produtos e Processos" },
            { valor: "B", texto: "Refatorar partes do código e automatizar testes para garantir que nada se quebre.", perfil: "Qualidade, Testes e Manutenção" },
            { valor: "C", texto: "Eu faria qualquer uma das duas, desde que alguém me dissesse claramente o que fazer.", perfil: "Nada" }
        ]
    },
    {
        id: "4",
        texto: "Seu grupo vai participar de uma competição onde o desafio é desenvolver uma solução para otimizar rotas de entrega. Com pouco tempo disponível, como você se sentiria mais útil nesse cenário?",
        opcoes: [
            { valor: "A", texto: "Trabalhando nos cálculos, buscando uma solução matemática eficaz.", perfil: "Fundamentos Matemáticos e Computacionais" },
            { valor: "B", texto: "Eu ajudaria no que fosse pedido, sem me importar tanto com a natureza da tarefa.", perfil: "Nada" },
            { valor: "C", texto: "Criando interfaces intuitivas e visualizações claras para mostrar os resultados aos avaliadores.", perfil: "Projeto e Análise de Software" }
        ]
    },
    {
        id: "5",
        texto: "Uma startup quer desenvolver um sistema que funcione para milhões de usuários, com alta performance e segurança. Antes de começar, é preciso decidir qual aspecto discutir primeiro. Qual você priorizaria?",
        opcoes: [
            { valor: "A", texto: "Arquitetura escalável, banco de dados eficiente e servidores bem distribuídos.", perfil: "Programação e Infraestrutura Técnica" },
            { valor: "B", texto: "Eu seguiria a decisão da maioria do time ou do líder do projeto.", perfil: "Nada" },
            { valor: "C", texto: "Entender o usuário final e mapear suas necessidades reais antes de projetar a infra.", perfil: "Projeto e Análise de Software" }
        ]
    },
    {
        id: "6",
        texto: "Um cliente não técnico quer saber se o sistema que sua equipe está desenvolvendo está 'funcionando direito'. Você tem poucas horas para preparar uma demonstração. Qual abordagem você usaria?",
        opcoes: [
            { valor: "A", texto: "Explicaria o funcionamento com fluxos simples e mostraria protótipos navegáveis.", perfil: "Projeto e Análise de Software" },
            { valor: "B", texto: "Mostraria os testes automatizados, os indicadores de cobertura e estabilidade.", perfil: "Qualidade, Testes e Manutenção" },
            { valor: "C", texto: "Eu faria algo misto, sem me comprometer com uma abordagem específica.", perfil: "Nada" }
        ]
    },
    {
        id: "7",
        texto: "Durante a faculdade, você participa de um projeto onde todos vão programar, mas os papéis são flexíveis. Sem combinar previamente, qual tipo de tarefa você assumiria espontaneamente?",
        opcoes: [
            { valor: "A", texto: "Organizar o planejamento, acompanhar o progresso e garantir comunicação eficaz entre os membros.", perfil: "Gestão de Produtos e Processos" },
            { valor: "B", texto: "Resolver problemas de lógica e estruturar o sistema para escalar com facilidade.", perfil: "Programação e Infraestrutura Técnica" },
            { valor: "C", texto: "Eu esperaria outra pessoa me atribuir uma tarefa antes de decidir sozinho.", perfil: "Nada" }
        ]
    },
    {
        id: "8",
        texto: "Sua equipe tem um sistema funcionando bem, mas as prioridades mudaram: agora é preciso manter e evoluir o software existente. Diante dessa mudança, qual seria sua prioridade?",
        opcoes: [
            { valor: "A", texto: "Analisar dados de uso, coletar feedback e planejar evoluções com base nas necessidades reais dos usuários.", perfil: "Gestão de Produtos e Processos" },
            { valor: "B", texto: "Criar estratégias de versionamento, testes de regressão e monitoramento de erros.", perfil: "Qualidade, Testes e Manutenção" },
            { valor: "C", texto: "Eu seguiria o que já estava sendo feito até alguém solicitar algo diferente.", perfil: "Nada" }
        ]
    },
    {
        id: "9",
        texto: "Diante de um problema complexo, sua primeira abordagem costuma ser…",
        opcoes: [
            { valor: "A", texto: "Quebrar o problema em partes menores, usar modelos matemáticos e resolver passo a passo.", perfil: "Fundamentos Matemáticos e Computacionais" },
            { valor: "B", texto: "Eu não teria um método definido — dependeria do dia ou da inspiração.", perfil: "Nada" },
            { valor: "C", texto: "Testar diferentes caminhos, observar padrões e ajustar conforme os resultados.", perfil: "Programação e Infraestrutura Técnica" }
        ]
    },
    {
        id: "10",
        texto: "Em um projeto com prazo apertado, onde há várias formas de atingir o objetivo, qual caminho você tende a escolher?",
        opcoes: [
            { valor: "A", texto: "Buscar soluções com base em fundamentos sólidos, mesmo que exijam mais tempo de desenvolvimento.", perfil: "Fundamentos Matemáticos e Computacionais" },
            { valor: "B", texto: "Escolher a solução prática que funcione rápido e possa ser ajustada depois.", perfil: "Programação e Infraestrutura Técnica" },
            { valor: "C", texto: "Eu faria o que parecesse mais fácil ou o que outro colega sugerisse primeiro.", perfil: "Nada" }
        ]
    }
];

function nextStep() {
    document.getElementById("step1").classList.remove("active");
    document.getElementById("step2").classList.add("active");
}

function validarPerguntas() {
    let todasRespondidas = true;
    const validationDiv = document.getElementById("validationMessages");

    // Verifica se todas as perguntas foram respondidas
    perguntas.forEach((p) => {
        const input = document.querySelector(`input[name="q${p.id}"]:checked`);
        if (!input) {
            todasRespondidas = false;
        }
    });

    if (!todasRespondidas) {
        validationDiv.innerHTML = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            Por favor, responda todas as perguntas antes de prosseguir.
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
        return false;
    }

    return true; // Permite o avanço
}

function refazer() {
    document.getElementById("step3").classList.remove("active");
    document.getElementById("step1").classList.add("active");
    document.getElementById("resultado-final").innerHTML = "";
}

const perguntasDiv = document.getElementById("perguntas");

perguntas.forEach((p, index) => {
    const div = document.createElement("div");
    div.className = "question";
    div.innerHTML = `
        <h4>${index + 1}. ${p.texto}</h4>
        <div class="rating">
            ${p.opcoes.map(opcao => `
                <div class="option">
                    <input type="radio" name="q${p.id}" id="q${p.id}_${opcao.valor}" value="${opcao.valor}" required>
                    <label for="q${p.id}_${opcao.valor}">${opcao.valor}) ${opcao.texto}</label>
                </div>
            `).join('')}
        </div>
    `;
    perguntasDiv.appendChild(div);
});
function enviarRespostas() {
    if (!validarPerguntas()) return;

    const respostas = perguntas.map(p => {
        const input = document.querySelector(`input[name="q${p.id}"]:checked`);
        const opcaoEscolhida = p.opcoes.find(o => o.valor === input.value);
        return {
            id: p.id,
            perfil: opcaoEscolhida ? opcaoEscolhida.perfil : "Nada"
        };
    });

    console.log("Respostas coletadas:", respostas);

    fetch('/Form/CalcularPerfil', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(respostas)
    })
        .then(res => {
            if (!res.ok) throw new Error('Erro na resposta do servidor');
            return res.json();
        })
        .then(resultado => {
            // Armazena o resultado para uso posterior
            calculatedProfileData = resultado;

            document.getElementById("step2").classList.remove("active");
            document.getElementById("step3").classList.add("active");

            const resultadoFinal = document.getElementById("resultado-final");

            let html = "<ul>";

            if (resultado.principal) {
                html += `<li><strong>Perfil Principal:</strong> ${resultado.principal}</li>`;
            } else {
                html += `<li><strong>Nenhum perfil principal encontrado.</strong></li>`;
            }

            if (resultado.secundarios.length > 0) {
                html += `<li><strong>Perfis Secundários:</strong> ${resultado.secundarios.join(', ')}</li>`;
            } else {
                html += `<li><strong>Nenhum perfil secundário encontrado.</strong></li>`;
            }

            html += "</ul>";

            resultadoFinal.innerHTML = html;
        })
        .catch(err => {
            console.error("Erro ao enviar respostas:", err);
            document.getElementById("validationMessages").innerHTML = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                Houve um problema ao processar suas respostas. Por favor, tente novamente.
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        });
}

function finalizarFormulario() {
    if (!calculatedProfileData) {
        console.error("Nenhum perfil calculado para salvar.");
        document.getElementById("validationMessages").innerHTML = `
      <div class="alert alert-warning alert-dismissible fade show" role="alert">
        Não foi possível salvar o perfil. Por favor, refaça o questionário.
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>
    `;
        return;
    }

    // Requisição para salvar o perfil
    fetch('/Form/SalvarPerfil', { // Este é o endpoint que criaremos no C#
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Principal: calculatedProfileData.principal,
            Secundarios: calculatedProfileData.secundarios
        })
    })
        .then(response => {
            if (!response.ok) {
                // Se a resposta HTTP não for 2xx (ex: 400, 500)
                return response.text().then(text => { throw new Error(text); }); // Tentar ler como texto para ver a mensagem de erro do servidor
            }
            return response.json(); // Esperamos um JSON do servidor
        })
        .then(data => {
            if (data.success) {
                // Sucesso: exibe mensagem e redireciona
                alert(data.message); // Ou usa um sistema de notificação mais elegante
                // Redireciona para a página inicial (ou para onde o servidor disser)
                window.location.href = data.redirectUrl || '/Home/Index';
            } else {
                // Erro: exibe mensagem na div de validação
                document.getElementById("validationMessages").innerHTML = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
          ${data.message || 'Erro desconhecido ao salvar perfil.'}
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
      `;
            }
        })
        .catch(error => {
            console.error("Erro ao finalizar formulário:", error);
            document.getElementById("validationMessages").innerHTML = `
      <div class="alert alert-danger alert-dismissible fade show" role="alert">
        Ocorreu um erro inesperado ao salvar o perfil. (${error.message})
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>
    `;
        });
}

// Adiciona o event listener ao botão ANTES de qualquer função que possa depender disso, idealmente no final do script ou em um DOMContentLoaded
document.addEventListener('DOMContentLoaded', () => {
    const btnTerminar = document.getElementById('btnTerminarFormulario');
    if (btnTerminar) {
        btnTerminar.addEventListener('click', finalizarFormulario);
    }
});

// Se preferir, pode manter o onclick="finalizarFormulario()" diretamente no HTML do botão e remover o addEventListener
// Mas o addEventListener é geralmente mais limpo e flexível.