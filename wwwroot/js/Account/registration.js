let registrationButton = document.getElementById("registration-button")
registrationButton.addEventListener("click", ev => {
    let message = {
        UserName: document.getElementById("login-input").value,
        Password: document.getElementById("password-input").value,
        ConfirmPassword: document.getElementById("password-confirm-input").value
    }
    return sendJsonMessage("https://localhost:5001/Registration", 'POST', message).then();
}); 