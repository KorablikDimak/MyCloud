async function sendJsonMessage (uri, method, message = "") {
    let init;
    if (message === "") {
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

async function sendCommonJsonMessage(uri, method, message) {
    let init = {
        method: method,
        headers: {
            'GroupName': FileLoader.groupLogin.Name,
            'GroupPassword': FileLoader.groupLogin.GroupPassword,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(message)
    }
    return await fetch(uri, init);
}

async function logout() {
    await sendJsonMessage("https://192.168.1.130/Logout", 'GET');
    window.location = "https://192.168.1.130/Account/Login";
}

function getUserPhoto(userPhotoId) {
    let userPhoto = document.getElementById(userPhotoId);
    sendJsonMessage("https://192.168.1.130/GetUserPhotoUrl", 'GET').then(response => {
        response.text().then(text => {
            userPhoto.src = text;
        });
    });
}

async function isNameUsed(name) {
    if (name.length > 3) {
        let response = await sendJsonMessage("https://192.168.1.130/IsUserNameUsed", 'POST', name)
        let json = await response.json();
        if (json === true) {
            dropTextLogin.classList.add("highlight");
            dropTextLogin.innerText = "Данное имя занято";
        }
        console.log(json);
    }
}