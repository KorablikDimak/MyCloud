let dropTextLogin = document.getElementById("drop-text-login");

let loginButton = document.getElementById("login-button");
loginButton.addEventListener("click", () => {
    let message = {
        UserName: document.getElementById("login-input").value,
        Password: document.getElementById("password-input").value
    };
    
    sendJsonMessage("https://localhost:5001/Login", 'POST', message)
        .then((response) => {
            console.log(response.status);
            if (response.status === 200) {
                window.location = "https://localhost:5001/Home/MyFiles";
            }
            else {
                dropTextLogin.classList.add("highlight");
                dropTextLogin.innerText = "Неверный логин или пароль";
            }
        });
});