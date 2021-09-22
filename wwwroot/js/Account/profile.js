let container = document.getElementById("container");
initPage();

function initPage() {
    initChangePhoto();
    initChangePersonality();
    initMyFilesButton();
    initGroupsViewButton();
    initChangePassword();
    initDeleteAccountButton();
    updatePage().then();
}

function initChangePhoto() {
    let changePhoto = document.getElementById("change-photo");
    changePhoto.addEventListener("click", () => {
        //TODO open manager for photo
    });
}

function initChangePersonality() {
    let confirmButton = document.getElementById("confirm-personality");
    let changePersonalityWindow = document.getElementById("change-personality-window");
    let changeData = document.getElementById("change-data");
    
    confirmButton.addEventListener("click", () => {
        changePersonality().then(() => {
            changePersonalityWindow.classList.remove('highlight');
            container.classList.remove('highlight');
            updatePage().then();
        });
    });
    
    changePersonalityWindow.addEventListener("click", ev => {
        const target = ev.target;
        if (target === changePersonalityWindow) {
            changePersonalityWindow.classList.remove('highlight');
            container.classList.remove('highlight');
        }
    });
    
    changeData.addEventListener("click", () => {
        changePersonalityWindow.classList.add('highlight');
        container.classList.add('highlight');
    });
}

async function changePersonality() {
    let message = {
        Id: 0,
        UserName: "",
        Surname: document.getElementById("surname-input").value,
        Name: document.getElementById("name-input").value,
    }
    let response = await sendJsonMessage("https://localhost:5001/ChangePersonality", 'PATCH', message)
    if (response.status === 200) {
        await changeUserName();
    }
}

async function changeUserName() {
    let message = document.getElementById("username-input").value;
    let userName = document.getElementById("username").value;
    if (message !== userName) {
        let response = await sendJsonMessage("https://localhost:5001/ChangeUserName", 'PATCH', message);
        if (response.status === 200) {
            await logout();
        }
    }
}

function initMyFilesButton() {
    let myFiles = document.getElementById("my-files");
    myFiles.addEventListener("click", () => {
        window.location = "https://localhost:5001/Home/MyFiles";
    });
}

function initGroupsViewButton() {
    let groupsView = document.getElementById("groups-view");
    groupsView.addEventListener("click", () => {
        //TODO groups
    });
}

function initChangePassword() {
    let confirmButton = document.getElementById("confirm");
    let changePasswordWindow = document.getElementById("change-password-window");
    let changePassword = document.getElementById("password-change");
    
    confirmButton.addEventListener("click", () => {
        let message = {
            OldPassword: document.getElementById("old-password").value,
            NewPassword: document.getElementById("new-password").value
        };
        sendJsonMessage("https://localhost:5001/ChangePassword", 'PATCH', message).then(response => {
            if (response.status === 200) logout().then();
        });
    });
    
    changePasswordWindow.addEventListener("click", ev => {
        const target = ev.target;
        if (target === changePasswordWindow) {
            changePasswordWindow.classList.remove('highlight');
            container.classList.remove('highlight');
        }
    });
    
    changePassword.addEventListener("click", () => {
        changePasswordWindow.classList.add('highlight');
        container.classList.add('highlight');
    });
}

function initDeleteAccountButton() {
    let deleteAccountButton = document.getElementById("delete-account");
    let deleteAccountWindow = document.getElementById("delete-account-window");
    let confirm = document.getElementById("confirm-delete");

    deleteAccountButton.addEventListener("click", () => {
        deleteAccountWindow.classList.add('highlight');
        container.classList.add('highlight');
    });

    deleteAccountWindow.addEventListener("click", ev => {
        const target = ev.target;
        if (target === deleteAccountWindow) {
            deleteAccountWindow.classList.remove('highlight');
            container.classList.remove('highlight');
        }
    });
    
    confirm.addEventListener("click", () => {
        deleteAccount().then();
    });
}

async function deleteAccount() {
    let message = document.getElementById("password-delete-input").value;
    let response = await sendJsonMessage("https://localhost:5001/DeleteAccount", "DELETE", message)
    if (response.status === 200) await logout();
}