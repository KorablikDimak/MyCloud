let groups = document.getElementById("groups");
let users = document.getElementById("users");
let groupWindow = document.getElementById("group-window");
let container = document.getElementById("container");

let confirmChangeButton = document.getElementById("confirm-change");
let confirmCreateButton = document.getElementById("confirm-create");
let confirmDeleteButton = document.getElementById("confirm-delete");
let confirmEnterButton = document.getElementById("confirm-enter");
let confirmButtons = [confirmChangeButton, confirmCreateButton, confirmDeleteButton, confirmEnterButton];

initPage().then();

async function initPage() {    
    groupWindow.addEventListener("click", ev => {
        const target = ev.target;
        if (target === groupWindow) {
            resetHighlight();
            updatePage().then();
        }
    });
    
    let changeButton = document.getElementById("change-group");
    changeButton.addEventListener("click", () => {
        setHighlight(confirmChangeButton);
    });
    
    let createButton = document.getElementById("create-group");
    createButton.addEventListener("click", () => {
        setHighlight(confirmCreateButton);
    });

    let deleteButton = document.getElementById("delete-group");
    deleteButton.addEventListener("click", () => {
        setHighlight(confirmDeleteButton);
    });

    let enterButton = document.getElementById("enter-group");
    enterButton.addEventListener("click", () => {
        setHighlight(confirmEnterButton);
    });
    
    confirmChangeButton.addEventListener("click", () => {
        const newName = document.getElementById("group-name").value;
        const newPassword = document.getElementById("group-password").value;
        
        let message = [{
                GroupName: document.getElementById("groups-name-text").innerText,
                GroupPassword: document.getElementById("groups-password-text").innerText
            },
            {
                GroupName: newName,
                GroupPassword: newPassword
            }];
        console.log(message);
        sendJsonMessage("https://localhost:5001/ChangeGroupName", 'Patch', message).then(response => {
            if (response.status === 200) {
                updateGroupData(newName, newPassword);
                updatePage().then();
            }
        });
    });
    
    confirmCreateButton.addEventListener("click", () => {
        createGroup().then(response => {
            if (response.status === 200) {
                resetHighlight();
                updatePage().then();
            }
        });
    });
    
    confirmDeleteButton.addEventListener("click", () => {
        deleteGroup().then(response => {
            if (response.status === 200) {
                resetGroupData();
                resetHighlight();
                updatePage().then();
            }
        });
    });
    
    confirmEnterButton.addEventListener("click", () => {
        enterTheGroup().then(response => {
            if (response.status === 200) {
                resetHighlight();
                updatePage().then();
            }
        });
    });

    let confirmLeaveButton = document.getElementById("leave-group");
    confirmLeaveButton.addEventListener("click", () => {
        if (confirm("Вы уверены, что хотите покинуть группу?")) {
            sendMessage("https://localhost:5001/LeaveFromGroup", 'DELETE').then(response => {
                if (response.status === 200) {
                    resetGroupData();
                    updatePage().then();
                }
            });
        }
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
    confirmButtons.forEach(function (button) {
        button.classList.remove("highlight");
    });
    groupWindow.classList.remove("highlight");
    container.classList.remove("highlight");
}

async function updatePage() {
    clearGroups();
    clearUsers();
    await updateGroups();
    await updateUsers();
}

async function enterTheGroup() {
    return await sendMessage("https://localhost:5001/EnterInGroup", 'POST');
}

async function deleteGroup() {
    return await sendMessage("https://localhost:5001/DeleteGroup", 'DELETE');
}

async function createGroup() {
    return await sendMessage("https://localhost:5001/CreateGroup", 'POST');
}

async function sendMessage(url, method) {
    let message = {
        GroupName: document.getElementById("group-name").value,
        GroupPassword: document.getElementById("group-password").value
    };
    return await sendJsonMessage(url, method, message);
}

function resetGroupData() {
    updateGroupData("", "");
}

function updateGroupData(name, password) {
    document.getElementById("groups-name-text").innerText = name;
    document.getElementById("groups-password-text").innerText = password;

    document.getElementById("group-name").value = name;
    document.getElementById("group-password").value = password;
}

function clearGroups() {
    groups.innerHTML = "";
}

async function updateGroups() {
    let response = await sendJsonMessage("https://localhost:5001/FindMyGroups", 'GET');
    let json = await response.json();
    updateGroupData(json[0].groupName, json[0].groupPassword);
    json.forEach(addGroup);
}

function addGroup(group) {
    let groupElement = document.createElement("div");
    groupElement.className = "group";
    groupElement.innerText = `${group.groupName}`;
    groupElement.addEventListener("click", () => {
        let groupElements = document.querySelectorAll(".group");
        groupElements.forEach(function (element) {
            element.classList.remove("highlight");
        });
        groupElement.classList.add("highlight");
        updateGroupData(group.groupName, group.groupPassword);
        clearUsers();
        updateUsers().then();
    });
    groups.append(groupElement);

    let line = document.createElement("hr");
    line.className = "horizontal-line";
}

function clearUsers() {
    users.innerHTML = "";
}

async function updateUsers() {
    let response = await sendMessage("https://localhost:5001/FindUsersInGroup", 'POST');
    let json = await response.json();
    json.forEach(addUser);
}

function addUser(user) {
    let userElement = document.createElement("div");
    userElement.className = "user";
    if (user.surname === "" && user.name === "") {
        userElement.innerText = `${user.userName}`;
    } 
    else {
        userElement.innerText = `${user.surname} ${user.name} (${user.userName})`;
    }
    users.append(userElement);
    
    let line = document.createElement("hr");
    line.className = "horizontal-line";
}