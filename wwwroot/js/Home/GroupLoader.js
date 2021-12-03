class GroupLoader {
    async findMyGroups() {
        let response = await sendJsonMessage(siteAddress + "FindMyGroups", 'GET');
        return await response.json();
    }

    showGroupsTable(json) {
        let group = document.createElement("div");
        group.className = "file";
        group.id = `group-${json.name}`;
        group.innerHTML =
            `<img  alt=\"\" src=\"${siteAddress}images/free-icon-folder-149334.png\" class=\"file-img\"/>\n` +
            `<div class=\"file-name\">${json.name}</div>\n`;
        group.groupPassword = json.groupPassword;
        return group;
    }

    showGroupsList(json) {
        let group = document.createElement("div");
        group.className = "file";
        group.id = `group-${json.name}`;
        group.classList.add("file-list");
        group.innerHTML =
            `<div class=\"file-name\">${json.name}</div>\n`;
        group.groupPassword = json.groupPassword;
        return group;
    }
    
    addClickEvent(content) {
        let element = document.getElementById(`group-${content.name}`);
        element.addEventListener("click", () => {
            FileLoader.groupLogin.Name = content.name;
            FileLoader.groupLogin.GroupPassword = content.groupPassword;
            pageUpdater.typeOfPage = "commonFiles";
            header.innerText = `Группа ${content.name}`;
            updatePage();
        });
    }
}