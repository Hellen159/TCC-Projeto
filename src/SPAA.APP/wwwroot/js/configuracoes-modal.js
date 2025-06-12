document.getElementById("formAlterarNome").onsubmit = function (e) {
    var nome = document.getElementById("NovoNome").value.trim();
    var validationDiv = document.getElementById("validationMessages");
    validationDiv.innerHTML = ""; // limpa mensagens anteriores
    if (nome === "") {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'O nome é obrigatório.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
    if (nome.length < 10 || nome.length > 150) {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'O nome deve ter entre 10 e 150 caracteres.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
};

// Validação do Formulário de Alterar Senha
document.getElementById("formAlterarSenha").onsubmit = function (e) {
    var senhaAtual = document.getElementById("SenhaAtual").value.trim();
    var novaSenha = document.getElementById("NovaSenha").value.trim();
    var confirmacaoSenha = document.getElementById("ConfirmacaoSenha").value.trim();
    var validationDiv = document.getElementById("validationMessages");
    validationDiv.innerHTML = ""; // Limpa mensagens anteriores
    if (senhaAtual === "") {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'A senha atual é obrigatória.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
    if (novaSenha === "" || confirmacaoSenha === "") {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'A nova senha e a confirmação são obrigatórias.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
    if (novaSenha.length < 8 || novaSenha.length > 20) {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'A nova senha deve ter entre 8 e 20 caracteres.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
    if (novaSenha !== confirmacaoSenha) {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'As senhas não coincidem.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
};

// Validação do Formulário de Importar Histórico
document.getElementById("formHistorico").onsubmit = function (e) {
    var historicoInput = document.getElementById("Historico");
    var historico = historicoInput.value;
    var validationDiv = document.getElementById("validationMessages");
    validationDiv.innerHTML = ""; // limpa mensagens anteriores
    if (!historico) {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'O arquivo de histórico é obrigatório.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
    var fileExtension = historico.split('.').pop().toLowerCase();
    if (fileExtension !== 'pdf') {
        e.preventDefault();
        validationDiv.innerHTML = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            'Apenas arquivos PDF são permitidos.' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
        return false;
    }
};