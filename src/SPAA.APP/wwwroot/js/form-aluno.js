const perguntas = [
    { id: "1", texto: "Qual o seu nível de interesse em compreender arquiteturas de software escaláveis ou distribuídas?", perfil: "Desenvolvimento Web" },
    { id: "2", texto: "Em que grau o desenvolvimento de aplicações web, tanto frontend quanto backend, se alinha ao seu perfil?", perfil: "Desenvolvimento Web" },
    { id: "3", texto: "Em que grau o estudo de inteligência artificial e aprendizado de máquina lhe parece atraente?", perfil: "Inteligência Artificial" },
    { id: "4", texto: "Em que medida você se sente atraído(a) pela automação de tarefas por meio da programação?", perfil: "Inteligência Artificial" },
    { id: "5", texto: "Você considera importante compreender o funcionamento interno de sistemas operacionais?", perfil: "Sistemas e Baixo Nível" },
    { id: "6", texto: "Qual é o seu interesse em compreender como funcionam compiladores e interpretadores de linguagens?", perfil: "Sistemas e Baixo Nível" },
    { id: "7", texto: "Em que medida o desenvolvimento de software de base, como bibliotecas, sistemas embarcados e ferramentas de infraestrutura, é relevante para você?", perfil: "Sistemas e Baixo Nível" },
    { id: "8", texto: "O quanto você considera estimulante o estudo de cálculo, álgebra linear e equações diferenciais?", perfil: "Computação Teórica" },
    { id: "9", texto: "Em que medida os fundamentos matemáticos de algoritmos e estruturas de dados lhe interessam?", perfil: "Computação Teórica" },
    { id: "10", texto: "Qual é o seu grau de interesse pelos aspectos teóricos da computação, como autômatos, linguagens formais e complexidade computacional?", perfil: "Computação Teórica" },
    { id: "11", texto: "Qual é a sua percepção sobre a relevância da engenharia de requisitos, análise e documentação técnica?", perfil: "Engenheiro de Software" },
    { id: "12", texto: "Qual é o seu interesse em trabalhar com metodologias ágeis, como Scrum ou Kanban, para o desenvolvimento de software?", perfil: "Engenheiro de Software" },
    { id: "13", texto: "Em que grau você se interessa pela automação de testes e pela garantia de qualidade do software durante o processo de desenvolvimento?", perfil: "Engenheiro de Software" },
    { id: "14", texto: "Em que medida resolver problemas de lógica e algoritmos lhe desperta interesse?", perfil: "Desempenho e Algoritmos" },
    { id: "15", texto: "Qual o seu nível de motivação para desenvolver sistemas de software com múltiplas funcionalidades e componentes?", perfil: "Desempenho e Algoritmos" },
    { id: "16", texto: "Você se sente motivado(a) a desenvolver algoritmos otimizados com foco em desempenho?", perfil: "Desempenho e Algoritmos" },
    { id: "17", texto: "Qual é o seu nível de interesse em participar de competições de programação, como maratonas e hackathons?", perfil: "Competição e Programação" },
    { id: "18", texto: "Você se sente motivado(a) por desafios em ambientes competitivos que exigem soluções criativas sob pressão?", perfil: "Competição e Programação" },
    { id: "19", texto: "Em que grau a área de segurança da informação, incluindo análise de vulnerabilidades e criptografia, chama sua atenção?", perfil: "Segurança e Hacking Ético" },
    { id: "20", texto: "Você tem curiosidade em explorar ambientes de simulação voltados à prática de hacking ético?", perfil: "Segurança e Hacking Ético" },
    { id: "21", texto: "Você tem interesse por sistemas embarcados veiculares, veículos autônomos ou controle e monitoramento de veículos?", perfil: "Engenharia Automotiva" },
    { id: "22", texto: "Você se interessa por drones, satélites, aviônica ou navegação autônoma?", perfil: "Engenharia Aeroespacial" },
    { id: "23", texto: "Você gostaria de atuar com monitoramento, controle e otimização energética com apoio de software?", perfil: "Engenharia de Energia" },
    { id: "24", texto: "Você tem interesse por circuitos, sensores, microcontroladores e dispositivos eletrônicos?", perfil: "Engenharia Eletrônica" },
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
        ${[...Array(10)].map((_, i) => `
                        <input type="radio" name="q${p.id}" id="q${p.id}_${i + 1}" value="${i + 1}" required>
                        <label for="q${p.id}_${i + 1}">${i + 1}</label>
                    `).join('')}
    </div>
    `;
    perguntasDiv.appendChild(div);
});

function enviarRespostas() {
    if (!validarPerguntas()) return;

    const respostas = perguntas.map(p => {
        const input = document.querySelector(`input[name="q${p.id}"]:checked`);
        return {
            id: p.id,
            nota: parseInt(input.value),
            perfil: p.perfil
        };
    });

    fetch('/Form/CalcularPerfil', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(respostas)
    })
        .then(res => res.json())
        .then(resultado => {
            document.getElementById("step2").classList.remove("active");
            document.getElementById("step3").classList.add("active");

            const resultadoFinal = document.getElementById("resultado-final");
            resultadoFinal.innerHTML = `
            <p><strong>Perfil Principal:</strong> ${resultado.principal}</p>
            <p><strong>Perfis Secundários:</strong> ${resultado.secundarios}</p>
            <p><strong>Perfis Terciários:</strong> ${resultado.terciarios}</p>
        `;
        })
        .catch(err => {
            console.error("Erro ao enviar respostas:", err);
        });
}