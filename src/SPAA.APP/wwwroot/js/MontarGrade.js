document.addEventListener("DOMContentLoaded", function () {
    const celulas = document.querySelectorAll(".celula");
    const inputOculto = document.getElementById("HorariosMarcados");
    const limparBtn = document.getElementById("limparBtn");

    let selecionadas = [];

    celulas.forEach(celula => {
        celula.addEventListener("click", function () {
            const horario = this.getAttribute("data-horario");
            const dia = this.getAttribute("data-dia");

            const codigo = `${dia}${horario}`;

            const index = selecionadas.indexOf(codigo);

            if (index === -1) {
                selecionadas.push(codigo);
                this.classList.add("selecionada");
            } else {
                selecionadas.splice(index, 1);
                this.classList.remove("selecionada");
            }

            inputOculto.value = JSON.stringify(selecionadas);
        });
    });

    limparBtn.addEventListener("click", function () {
        selecionadas = [];
        inputOculto.value = '';
        celulas.forEach(celula => {
            celula.classList.remove("selecionada");
        });
    });
});