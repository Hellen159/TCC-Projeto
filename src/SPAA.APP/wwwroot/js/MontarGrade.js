﻿document.addEventListener("DOMContentLoaded", function () {
    const celulas = document.querySelectorAll(".celula");
    const inputOculto = document.getElementById("HorariosMarcados");
    const limparBtn = document.getElementById("limparBtn");
    let selecionadas = [];

    // === Função auxiliar: Verifica se há conflito ===
    function temConflito(horario) {
        return selecionadas.includes(horario);
    }

    // === Preencher seleção com turmas salvas ===
    const turmasSalvasCodigos = window.turmasSalvasCodigos || [];

    // Função para marcar checkboxes com base nos códigos salvos
    function marcarCheckboxesSalvas() {
        document.querySelectorAll('input[name="turmasSelecionadas"]').forEach(checkbox => {
            const codigoUnico = parseInt(checkbox.dataset.codigounicoturma);

            if (turmasSalvasCodigos.includes(codigoUnico)) {
                checkbox.checked = true;

                // Simular o evento change para atualizar grade
                checkbox.dispatchEvent(new Event("change"));
            }
        });
    }

    window.addEventListener("load", function () {
        console.log("Turmas salvas no JS:", turmasSalvasCodigos);
        marcarCheckboxesSalvas();
    });

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
                    const validationDiv = document.getElementById("validationMessages");
                    validationDiv.innerHTML = `
                    <div class="alert alert-danger alert-dismissible fade show" role="alert">
                        Conflito de horário!
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>`;
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
    limparBtn.addEventListener("click", async function () {
        selecionadas = [];
        inputOculto.value = '';
        celulas.forEach(celula => {
            celula.classList.remove("selecionada", "turma-selecionada");
            celula.removeAttribute("title");
        });

        // Desmarcar checkboxes
        document.querySelectorAll('input[name="turmasSelecionadas"]').forEach(cb => cb.checked = false);

        // Recarregar as matérias (chamada AJAX)
        try {
            const response = await fetch('/Grid/MontarGrade', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                location.reload(); // Recarrega a página com os dados atualizados
            } else {
                console.error('Erro ao recarregar as matérias');
            }
        } catch (error) {
            console.error('Erro na requisição:', error);
        }
    });

    // === Evento para checkboxes de turmas ===
    document.querySelectorAll('input[name="turmasSelecionadas"]').forEach(checkbox => {
        checkbox.addEventListener("change", function () {
            const linhaTurma = this.closest(".linha");
            const horarioString = linhaTurma.querySelector(".celulas:nth-child(3)").textContent.trim();
            const codigoDisciplina = linhaTurma.querySelector(".celulas:nth-child(2)").textContent.trim();
            const nomeDisciplina = linhaTurma.querySelector(".celulas:nth-child(1)").textContent.trim();

            if (!horarioString || horarioString === "-") return;

            const checkboxesMarcados = document.querySelectorAll('input[name="turmasSelecionadas"]:checked');
            for (let cb of checkboxesMarcados) {
                if (cb !== this && cb.dataset.nomedisciplina === nomeDisciplina) {
                    // Mostra mensagem de erro estilizada
                    const validationDiv = document.getElementById("validationMessages");
                    validationDiv.innerHTML = `
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    Você já selecionou outra turma da disciplina: <strong>${nomeDisciplina}</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>`;

                    this.checked = false; // Desmarca automaticamente
                    return;
                }
            }

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
                const validationDiv = document.getElementById("validationMessages");
                validationDiv.innerHTML = ""; // Limpa mensagens anteriores
                validationDiv.innerHTML = `
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    Conflito de horário! Esta turma tem horário sobreposto com outro já marcado: ${horarioString}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>`;
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
    // === BOTÃO SALVAR GRADE ===
    document.getElementById('salvarGradeBtn').addEventListener('click', function () {
        const turmasSelecionadas = [];

        // Coleta todas as checkboxes marcadas
        document.querySelectorAll('input[name="turmasSelecionadas"]:checked').forEach(checkbox => {
            // Acessa os dados diretamente dos data-attributes do checkbox
            turmasSelecionadas.push({
                CodigoTurmaUnico: parseInt(checkbox.dataset.codigounicoturma), // Use parseInt para garantir que é um número
                CodigoDisciplina: checkbox.dataset.codigodisciplina,
                NomeDisciplina: checkbox.dataset.nomedisciplina,
                Horario: checkbox.dataset.horario,
                NomeProfessor: checkbox.dataset.nomeprofessor
            });
        });

        // Envia para o controller via AJAX
        fetch('/Grid/SalvarGrade', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(turmasSelecionadas)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.location.href = "/"; // Redireciona diretamente para a Home
                } else {
                    alert("Erro: " + (data.message || "Falha ao salvar grade."));
                }
            })
            .catch(error => {
                alert("Erro na requisição: " + error.message);
                console.error("Erro na requisição AJAX:", error);
            });
    });
});
