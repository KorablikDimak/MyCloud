async function sendJsonMessage (uri, method, message = null) {
    let init;
    if (message == null) {
        init = {
            method: method
        }
    }
    else {
        init = {
            method: method,
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(message)
        }
    }
    
    return await fetch(uri, init);
}

async function logout() {
    await sendJsonMessage("https://localhost:5001/Home/Logout", 'GET')
    window.location = "https://localhost:5001/Account/Login";
}