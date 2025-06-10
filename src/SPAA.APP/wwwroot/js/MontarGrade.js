document.addEventListener("DOMContentLoaded", function () {
    const celulas = document.querySelectorAll(".celula");
    const inputOculto = document.getElementById("HorariosMarcados");
    const limparBtn = document.getElementById("limparBtn");
    let selecionadas = [];

    // === Função auxiliar: Verifica se há conflito ===
    function temConflito(horario) {
        return selecionadas.includes(horario);
    }

    // === Seleção de células na grade ===
    celulas.forEach(celula => {
        celula.addEventListener("click", function () {
            const horario = this.getAttribute("data-horario").trim();
            const dia = this.getAttribute("data-dia");
            const codigo = `${dia}${horario}`;
            const index = selecionadas.indexOf(codigo);

            if (index === -1) {
                // Verificar se já existe alguma seleção nesse horário
                if (temConflito(codigo)) {
                    alert("Conflito de horário!");
                    return;
                }

                selecionadas.push(codigo);
                this.classList.add("selecionada");
            } else {
                selecionadas.splice(index, 1);
                this.classList.remove("selecionada");
            }

            inputOculto.value = JSON.stringify([...new Set(selecionadas)]);
        });
    });

    // === Botão Limpar Seleção ===
    limparBtn.addEventListener("click", function () {
        selecionadas = [];
        inputOculto.value = '';
        celulas.forEach(celula => {
            celula.classList.remove("selecionada", "turma-selecionada");
            celula.removeAttribute("title");
        });

        // Desmarcar checkboxes
        document.querySelectorAll('input[name="turmasSelecionadas"]').forEach(cb => cb.checked = false);
    });

    // === Evento para checkboxes de turmas ===
    document.querySelectorAll('input[name="turmasSelecionadas"]').forEach(checkbox => {
        checkbox.addEventListener("change", function () {
            const linhaTurma = this.closest(".linha");
            const horarioString = linhaTurma.querySelector(".celulas:nth-child(3)").textContent.trim();
            const codigoDisciplina = linhaTurma.querySelector(".celulas:nth-child(2)").textContent.trim();

            if (!horarioString || horarioString === "-") return;

            // Função para parse do horário da turma
            function parseHorarioTurma(horarioString) {
                const horarios = [];

                // Separa por vírgulas e processa cada parte
                horarioString.split(',').forEach(part => {
                    part = part.trim(); // Remove espaços extras
                    const match = part.match(/^(\d+)([MTN])(\d+)$/i);

                    if (!match) return;

                    const dias = match[1].split('');
                    const turno = match[2];
                    const blocos = match[3].split('');

                    for (const dia of dias) {
                        for (const bloco of blocos) {
                            horarios.push(`${dia}${turno}${bloco}`);
                        }
                    }
                });

                return horarios;
            }

            const horariosConvertidos = parseHorarioTurma(horarioString);

            // Verificar se algum horário da turma já foi marcado manualmente
            const haConflito = horariosConvertidos.some(horario => temConflito(horario));

            if (haConflito && this.checked) {
                alert("Conflito de horário! Esta turma tem horário sobreposto com outro já marcado.");
                this.checked = false; // Desmarca automaticamente
                return;
            }

            horariosConvertidos.forEach(codigo => {
                const [dia, horario] = [codigo[0], codigo.slice(1)];
                const celula = document.querySelector(`.celula[data-dia="${dia}"][data-horario="${horario}"]`);

                if (celula) {
                    if (this.checked) {
                        // Adicionar à lista de selecionadas
                        if (!selecionadas.includes(codigo)) {
                            selecionadas.push(codigo);
                        }

                        // Remover seleção manual (evita X)
                        celula.classList.remove("selecionada");

                        // Adicionar estilo específico para turma
                        celula.classList.add("turma-selecionada");
                        celula.setAttribute("title", codigoDisciplina); // Tooltip opcional
                    } else {
                        // Remover da lista
                        const index = selecionadas.indexOf(codigo);
                        if (index > -1) {
                            selecionadas.splice(index, 1);
                        }

                        // Remover marcação visual
                        celula.classList.remove("turma-selecionada");
                        celula.removeAttribute("title");
                    }
                }
            });

            inputOculto.value = JSON.stringify([...new Set(selecionadas)]);
        });
    });
});