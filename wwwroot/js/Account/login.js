let dropTextLogin = document.getElementById("drop-text-login");

let loginButton = document.getElementById("login-button");
loginButton.addEventListener("click", () => {
    let message = {
        UserName: document.getElementById("login-input").value,
        Password: document.getElementById("password-input").value
    };
    
    sendJsonMessage("https://192.168.1.130/Login", 'POST', message)
        .then((response) => {
            console.log(response.status);
            if (response.status === 200) {
                window.location = "https://192.168.1.130/Home/MyFiles";
            }
            else {
                dropTextLogin.classList.add("highlight");
                dropTextLogin.innerText = "Неверный логин или пароль";
            }
        });
});