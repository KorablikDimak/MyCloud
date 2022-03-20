let loginInput = document.getElementById("login-input");
let dropTextLogin = document.getElementById("drop-text-login");

loginInput.addEventListener("input", () => {
    if (loginInput.value.length < 3) {
        dropTextLogin.classList.add("highlight");
        dropTextLogin.innerText = "Минимальная длина: 3 символа";
    }
    else if (loginInput.value.length > 20) {
        dropTextLogin.classList.add("highlight");
        dropTextLogin.innerText = "Максимальная длина: 20 символов";
    }    
    else {
        dropTextLogin.classList.remove("highlight");
    }
    isNameUsed(loginInput.value).then();
});

async function isNameUsed(name) {
    if (name.length > 3) {
        let response = await sendJsonMessage(siteAddress + "IsGroupNameUsed", 'POST', name)
        let json = await response.json();
        if (json === true) {
            dropTextLogin.classList.add("highlight");
            dropTextLogin.innerText = "Данное имя занято";
        }
    }
}

let passwordInput = document.getElementById("password-input");
let dropTextPassword = document.getElementById("drop-text-password");
passwordInput.addEventListener("input", () => {
    if (passwordInput.value.length < 8) {
        dropTextPassword.classList.add("highlight");
        dropTextPassword.innerText = "Минимальная длина: 8 символов";
    }
    else if (passwordInput.value.length > 20) {
        dropTextPassword.classList.add("highlight");
        dropTextPassword.innerText = "Максимальная длина: 20 символов";
    }
    else {
        dropTextPassword.classList.remove("highlight");
    }
});

let confirmPasswordInput = document.getElementById("password-confirm-input");
let dropTextPasswordConfirm = document.getElementById("drop-text-password-confirm");
confirmPasswordInput.addEventListener("input", () => {
    if (passwordInput.value !== confirmPasswordInput.value) {
        dropTextPasswordConfirm.classList.add("highlight");
        dropTextPasswordConfirm.innerText = "Пароли не совпадают";
    }
    else {
        dropTextPasswordConfirm.classList.remove("highlight");
    }
});

let registrationButton = document.getElementById("registration-button");
registrationButton.addEventListener("click", () => {
    let message = {
        UserName: loginInput.value,
        Password: passwordInput.value,
        ConfirmPassword: confirmPasswordInput.value
    }
    
    sendJsonMessage(siteAddress + "Registration", 'POST', message)
        .then(() => {window.location = siteAddress + "Account/Login";
    });
});