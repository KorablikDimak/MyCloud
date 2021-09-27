let commonButton = document.getElementById("common-files");
commonButton.addEventListener("click", () => {
    //TODO common files
});

async function loadMyGroups() {
    let response = await sendJsonMessage("https://localhost:5001/FindMyGroups", 'GET');
    let json = await response.json();
    json.forEach();
}

function showGroupTable(json) {
    let group = document.createElement("div");
    group.className = "file";
    group.innerHTML = 
        `<img  alt=\"\" src=\"https://localhost:5001/images/free-icon-folder-149334.png\" class=\"file-img\"/>\n` +
        `<div class=\"file-name\">${json.groupName}</div>\n`;
    group.groupPassword = json.groupPassword;
    return group;
}

function showGroupList(json) {
    let file = document.createElement("div");
    file.className = "file";
    file.classList.add("file-list")
    file.innerHTML = 
        `<div class=\"file-name\">${json.groupName}</div>\n`;
    group.groupPassword = json.groupPassword;
    return file;
}