document.addEventListener("DOMContentLoaded", function () {
    const btnEditar = document.getElementById("btn-adicionar-tarefa");
    const modal = document.getElementById("modal-overlay");
    const gradeOriginal = document.querySelector(".tabela-grade table");
    const gradeEditavel = document.getElementById("grade-editavel");
    const btnCancelar = document.getElementById("btn-cancelar");
    const btnSalvar = document.getElementById("btn-salvar");

    let gradeInputs = [];

    // Função para clonar a tabela e transformar células vazias em inputs
    function criarGradeEditavel() {
        const novaGrade = gradeOriginal.cloneNode(true);
        const cells = novaGrade.querySelectorAll("td");

        gradeInputs = [];

        cells.forEach(cell => {
            if (cell.textContent.trim() === "---") {
                cell.classList.add("td-editavel");
                const input = document.createElement("input");
                input.type = "text";
                input.placeholder = "Tarefa";
                cell.innerHTML = "";
                cell.appendChild(input);
                gradeInputs.push({ celula: cell, horario: getHorarioFromCell(cell) });
            }
        });

        gradeEditavel.innerHTML = "";
        gradeEditavel.appendChild(novaGrade);

        // Adiciona classe específica para scroll
        gradeEditavel.classList.add("tabela-modal1");
    }

    // Extrair o horário do dia e bloco
    function getHorarioFromCell(cell) {
        const row = cell.parentNode; // <tr>
        const table = row.parentNode;
        const rows = Array.from(table.querySelectorAll("tr")); // todas as tr
        const rowIndex = rows.indexOf(row); // índice global da linha

        const cells = Array.from(row.querySelectorAll("td"));
        const cellIndex = cells.indexOf(cell);

        if (cellIndex <= 0) return null;

        // Dias da semana em formato UnB (Seg=2, ..., Sáb=7)
        const diasUnb = [2, 3, 4, 5, 6, 7]; // Seg(0)=2, ..., Sáb(5)=7
        const diaUnb = diasUnb[cellIndex - 1]; // pula a primeira coluna

        if (!diaUnb) return null;

        // Blocos de horário com base na linha (índice 0 = 08:00)
        const blocos = [
            "M1", "M2", "M3", "M4", "M5",   // Manhã (08:00 - 12:00)
            "T1", "T2", "T3", "T4", "T5", "T6", // Tarde (13:00 - 18:00)
            "N1"                            // Noite (19:00)
        ];

        const blocoUnb = blocos[rowIndex];

        if (!blocoUnb || !diaUnb) return null;

        return `${diaUnb}${blocoUnb}`;
    }

    btnEditar.addEventListener("click", function () {
        criarGradeEditavel();
        modal.style.display = "block";
    });

    btnCancelar.addEventListener("click", function () {
        modal.style.display = "none";
    });

    btnSalvar.addEventListener("click", function () {
        const tarefas = gradeInputs.map(item => ({
            horario: item.horario,
            descricao: item.celula.querySelector("input").value.trim()
        })).filter(t => t.descricao !== "");

        fetch('/Home/SalvarTarefas', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(tarefas)
        })
            .then(response => response.json())
            .then(data => {
                alert('Tarefas salvas com sucesso!');
                modal.style.display = "none";
            })
            .catch(error => {
                console.error('Erro ao salvar:', error);
                alert('Erro ao salvar tarefas.');
            });
    });
});