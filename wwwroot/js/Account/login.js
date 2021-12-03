let dropTextLogin = document.getElementById("drop-text-login");

let registrationButton = document.getElementById("registration-button");
registrationButton.addEventListener("click", () => {
    window.location = siteAddress + "Account/Registration";
});

let loginButton = document.getElementById("login-button");
loginButton.addEventListener("click", () => {
    let message = {
        UserName: document.getElementById("login-input").value,
        Password: document.getElementById("password-input").value
    };
    
    sendJsonMessage(siteAddress + "Login", 'POST', message)
        .then((response) => {
            console.log(response.status);
            if (response.status === 200) {
                window.location = siteAddress + "Home/MyFiles";
            }
            else {
                dropTextLogin.classList.add("highlight");
                dropTextLogin.innerText = "Неверный логин или пароль";
            }
        });
});