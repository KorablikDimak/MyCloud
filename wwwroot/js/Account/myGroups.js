const groups = document.getElementById("groups");
const users = document.getElementById("users");
const groupWindow = document.getElementById("group-window");
const groupWindowChange = document.getElementById("group-window-change");
const container = document.getElementById("container");

const confirmChangeButton = document.getElementById("confirm-change");
const confirmCreateButton = document.getElementById("confirm-create");
const confirmDeleteButton = document.getElementById("confirm-delete");
const confirmEnterButton = document.getElementById("confirm-enter");
const confirmLeaveButton = document.getElementById("confirm-leave");
const confirmButtons = [confirmChangeButton, confirmCreateButton, confirmDeleteButton, confirmEnterButton, confirmLeaveButton];

const groupName = document.getElementById("group-name");
const dropTextLogin = document.getElementById("drop-text-login");

const oldName = document.getElementById("group-name-old");
const groupNameNew = document.getElementById("group-name-new");
const dropTextLoginChange = document.getElementById("drop-text-login-change");

const groupPassword = document.getElementById("group-password");
const dropTextPassword = document.getElementById("drop-text-password");

const oldPassword = document.getElementById("group-password-old");
const groupPasswordNew = document.getElementById("group-password-new");
const dropTextPasswordChange = document.getElementById("drop-text-password-change");

function addValidateLenghtListener(button, dropText, minLenght, maxLenght) {
    button.addEventListener("input", () => {
        if (button.value.length < minLenght) {
            dropText.classList.add("highlight");
            dropText.innerText = `Минимальная длина: ${minLenght} символов`;
        }
        else if (button.value.length > maxLenght) {
            dropText.classList.add("highlight");
            dropText.innerText = `Максимальная длина: ${maxLenght} символов`;
        }
        else {
            dropText.classList.remove("highlight");
        }
    });
}

async function isNameUsed(name = groupName.value, dropText = dropTextLogin) {
    if (name.length > 3) {
        let response = await sendJsonMessage(siteAddress + "IsGroupNameUsed", 'POST', name)
        let json = await response.json();
        if (json === true) {
            dropText.classList.add("highlight");
            dropText.innerText = "Данное имя занято";
        }
    }
}

initPage().then();

async function initPage() {
    addValidateLenghtListener(groupName, dropTextLogin, 3, 20);
    addValidateLenghtListener(groupNameNew, dropTextLoginChange, 3, 20);
    
    groupNameNew.addEventListener("input", () => {
        isNameUsed(groupNameNew.value, dropTextLoginChange).then();
    });

    addValidateLenghtListener(groupPassword, dropTextPassword, 8, 32);
    addValidateLenghtListener(groupPasswordNew, dropTextPasswordChange, 8, 32);
    
    let exitButton = document.querySelector(".exit-button");
    exitButton.addEventListener("click", () => {
        window.location = siteAddress + "Account/Profile";
    });
    
    groupWindow.addEventListener("click", ev => {        
        const target = ev.target;
        if (target === groupWindow) {
            resetHighlight();
            updatePage().then();
        }
    });

    groupWindowChange.addEventListener("click", ev => {
        const target = ev.target;
        if (target === groupWindowChange) {
            resetHighlight();
            updatePage().then();
        }
    });
    
    let changeButton = document.getElementById("change-group");
    changeButton.addEventListener("click", () => {        
        confirmChangeButton.classList.add("highlight");
        groupWindowChange.classList.add("highlight");
        container.classList.add("highlight");
    });
    
    let createButton = document.getElementById("create-group");
    createButton.addEventListener("click", () => {
        groupName.value = "";
        groupName.addEventListener("input", checkName, true);
        setHighlight(confirmCreateButton);
    });

    let deleteButton = document.getElementById("delete-group");
    deleteButton.addEventListener("click", () => {
        setHighlight(confirmDeleteButton);
    });

    let enterButton = document.getElementById("enter-group");
    enterButton.addEventListener("click", () => {
        groupName.value = "";
        setHighlight(confirmEnterButton);
    });

    let leaveButton = document.getElementById("leave-group");
    leaveButton.addEventListener("click", () => {
        setHighlight(confirmLeaveButton);
    });
    
    confirmChangeButton.addEventListener("click", () => {
        let message = [{
                Name: oldName.value,
                GroupPassword: oldPassword.value
            },
            {
                Name: groupNameNew.value,
                GroupPassword: groupPasswordNew.value
            }];
        
        sendJsonMessage(siteAddress + "ChangeGroupLogin", 'Patch', message).then(response => {
            if (response.status === 200) {
                updatePage().then();
            }
        });
        resetHighlight();
        updateGroupData(groupNameNew.value);
    });
    
    confirmCreateButton.addEventListener("click", () => {        
        createGroup().then(response => {
            if (response.status === 200) {
                updatePage().then();
            }
        });
        resetHighlight();
    });
    
    confirmDeleteButton.addEventListener("click", () => {
        deleteGroup().then(response => {
            if (response.status === 200) {
                updatePage().then();
            }
        });
        resetGroupData();
        resetHighlight();
    });
    
    confirmEnterButton.addEventListener("click", () => {
        enterTheGroup().then(response => {
            if (response.status === 200) {
                updatePage().then();
            }
        });
        resetHighlight();
    });
    
    confirmLeaveButton.addEventListener("click", () => {
        leaveTheGroup().then(response => {
            if (response.status === 200) {
                updatePage().then();
            }
        });
        resetGroupData();
        resetHighlight();
    });

    updatePage().then(() => {
        let firsGroup = document.querySelector(".group");
        firsGroup.classList.add("highlight");
    });
}

function setHighlight(button) {
    button.classList.add("highlight");
    groupWindow.classList.add("highlight");
    container.classList.add("highlight");
}

function resetHighlight() {
    groupName.removeEventListener("input", checkName, true);
    confirmButtons.forEach(function (button) {
        button.classList.remove("highlight");
    });
    groupWindow.classList.remove("highlight");
    groupWindowChange.classList.remove("highlight");
    container.classList.remove("highlight");

    dropTextLoginChange.classList.remove("highlight");
    dropTextPasswordChange.classList.remove("highlight");

    groupNameNew.value = "";
    groupPasswordNew.value = "";
    oldPassword.value = "";
    groupPassword.value = "";

    dropTextLogin.classList.remove("highlight");
    dropTextPassword.classList.remove("highlight");
}

function checkName() {
    isNameUsed().then();
}

async function updatePage() {
    clearGroups();
    clearUsers();
    await updateGroups();
    await updateUsers();
}

async function enterTheGroup() {
    return await sendMessage(siteAddress + "EnterInGroup", 'POST');
}

async function deleteGroup() {
    return await sendMessage(siteAddress + "DeleteGroup", 'DELETE');
}

async function createGroup() {
    return await sendMessage(siteAddress + "CreateGroup", 'POST');
}

async function leaveTheGroup() {
    return await sendMessage(siteAddress + "LeaveFromGroup", 'DELETE');
}

async function sendMessage(url, method) {
    let message = {
        Name: groupName.value,
        GroupPassword: groupPassword.value
    };
    return await sendJsonMessage(url, method, message);
}

function resetGroupData() {
    updateGroupData("");
}

function updateGroupData(name) {
    document.getElementById("groups-name-text").innerText = name;
    groupName.value = name;
    oldName.value = name;
}

function clearGroups() {
    groups.innerHTML = "";
}

async function updateGroups() {
    let response = await sendJsonMessage(siteAddress + "FindMyGroups", 'GET');
    let json = await response.json();
    updateGroupData(json[0].name);
    json.forEach(addGroup);
}

function addGroup(group) {
    let groupElement = document.createElement("div");
    groupElement.className = "group";
    groupElement.innerText = `${group.name}`;
    groupElement.addEventListener("click", () => {
        let groupElements = document.querySelectorAll(".group");
        groupElements.forEach(function (element) {
            element.classList.remove("highlight");
        });
        groupElement.classList.add("highlight");
        updateGroupData(group.name);
        clearUsers();
        updateUsers().then();
    });
    groups.append(groupElement);
}

function clearUsers() {
    users.innerHTML = "";
}

async function updateUsers() {
    let response = 
        await sendJsonMessage(siteAddress + "FindUsersInGroup", 'POST', groupName.value);
    let json = await response.json();
    json.forEach(addUser);
}

function addUser(user) {
    let userElement = document.createElement("div");
    userElement.className = "user";
    
    if (user.surname == null) user.surname = "";
    if (user.name == null) user.name = "";
    
    if (user.surname === "" && user.name === "") {
        userElement.innerText = `${user.userName}`;
    } 
    else {
        userElement.innerText = `${user.surname} ${user.name} (${user.userName})`;
    }
    users.append(userElement);
}