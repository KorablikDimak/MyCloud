let changePhoto = document.getElementById("change-photo");
changePhoto.addEventListener("click", ev => {
    //TODO open manager for photo
});

let changeData = document.getElementById("change-data");
changeData.addEventListener("click", ev => {
    //TODO change data
});

let myFiles = document.getElementById("my-files");
myFiles.addEventListener("click", ev => {
    window.location = "https://localhost:5001/Home/MyFiles";
});

let groupsView = document.getElementById("groups-view");
groupsView.addEventListener("click", ev => {
    //TODO groups
});

let confirmButton = document.getElementById("confirm");
confirmButton.addEventListener("click", ev => {
    let message = {
        OldPassword: document.getElementById("old-password").value,
        NewPassword: document.getElementById("new-password").value
    };
    sendJsonMessage("https://localhost:5001/ChangePassword", 'PATCH', message).then(response => {
        if (response.status === 200) logout().then();
    });
});

let changePassword = document.getElementById("password-change");
changePassword.addEventListener("click", ev => {
    let container = document.getElementById("container");
    container.classList.add('highlight');
});

let deleteAccount = document.getElementById("delete-account");
deleteAccount.addEventListener("click", ev => {
    if (confirm("Вы уверены, что хотите удалить свой аккаунт вместе со всеми файлами?\n" +
        "Восстановить аккаунт будет невозможно.")) {
        sendJsonMessage("https://localhost:5001/DeleteAccount", "DELETE").then(response => {
            if (response.status === 200) logout().then();
        });
    }
});