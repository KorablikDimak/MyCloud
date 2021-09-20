let loginButton = document.getElementById("login-button");
loginButton.addEventListener("click", ev => {
    let message = {
        UserName: document.getElementById("login-input").value,
        Password: document.getElementById("password-input").value
    }
    
    sendJsonMessage("https://localhost:5001/Login", 'POST', message)
        .then(r => window.location = "https://localhost:5001/Home/MyFiles");
});