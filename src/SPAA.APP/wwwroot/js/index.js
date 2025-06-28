document.addEventListener("DOMContentLoaded", function () {
    const btnEditar = document.getElementById("btn-adicionar-tarefa");
    const modal = document.getElementById("modal-overlay");
    const gradeOriginal = document.querySelector(".tabela-grade table");
    const gradeEditavel = document.getElementById("grade-editavel");
    const btnCancelar = document.getElementById("btn-cancelar");
    const btnSalvar = document.getElementById("btn-salvar");

    let gradeInputs = [];

    function criarGradeEditavel() {
        const novaGrade = gradeOriginal.cloneNode(true);
        const cells = novaGrade.querySelectorAll("td");
        gradeInputs = [];

        cells.forEach(cell => {
            if (cell.classList.contains("horario")) return;

            const horario = cell.getAttribute("data-horario");
            const textoOriginal = cell.textContent.trim();
            const isTurma = textoOriginal.includes(" ");

            console.log("Processando célula:", cell);

            if (isTurma) {
                console.log("É uma turma, ignorando...");
                return;
            }

            cell.innerHTML = "";
            const container = document.createElement("div");
            container.className = "td-tarefa"; // Adiciona a classe td-tarefa
            container.style.position = "relative";

            const input = document.createElement("input");
            input.type = "text";
            input.placeholder = "Tarefa";
            input.style.width = "100%";
            input.style.textAlign = "center";
            input.style.fontSize = "0.8rem";
            input.style.border = "none";
            input.style.background = "transparent";
            input.value = textoOriginal === "---" ? "" : textoOriginal;

            cell.classList.add("td-editavel");

            if (input.value !== "") {
                console.log("Criando botão 'X' para:", input.value);

                const btnExcluir = document.createElement("span");
                btnExcluir.className = "btn-excluir-tarefa";
                btnExcluir.textContent = "x";
                btnExcluir.setAttribute("title", "Remover tarefa");

                btnExcluir.addEventListener("click", function (e) {
                    e.stopPropagation();
                    input.value = "";
                    cell.classList.add("td-editavel");
                    console.log("Campo limpo com sucesso!");
                });

                container.appendChild(input);
                container.appendChild(btnExcluir);
            } else {
                container.appendChild(input);
            }

            cell.appendChild(container);
            gradeInputs.push({ celula: cell, horario, input });
        });

        gradeEditavel.innerHTML = "";
        gradeEditavel.appendChild(novaGrade);
        gradeEditavel.classList.add("tabela-modal1");
    }

    btnEditar.addEventListener("click", function () {
        criarGradeEditavel();
        modal.style.display = "block";
    });

    btnCancelar.addEventListener("click", function () {
        modal.style.display = "none";
    });

    btnSalvar.addEventListener("click", function () {
        const tarefas = gradeInputs
            .map(item => {
                const valor = item.input.value.trim();
                if (valor === "") return null;
                return {
                    horario: item.horario,
                    descricao: valor
                };
            })
            .filter(t => t !== null);

        fetch('/Home/SalvarTarefas', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(tarefas)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Tarefas salvas com sucesso!');
                    modal.style.display = "none";
                    location.reload(); // Recarrega pra pegar dados atualizados
                } else {
                    alert('Erro ao salvar tarefas.');
                }
            })
            .catch(error => {
                console.error('Erro:', error);
                alert('Erro ao salvar tarefas.');
            });
    });
});