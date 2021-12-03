class FileLoader {
    static groupLogin = {
        Name: "",
        GroupPassword: ""
    };
    
    sortType = {
        OrderBy: "name",
        TypeOfSort: "ASC"
    };

    async getFileInfo() {
        let response = await sendJsonMessage(siteAddress + "GetFileInfo", 'POST', this.sortType);
        return await response.json();
    }

    async getCommonFileInfo() {
        let response = 
            await sendCommonJsonMessage(siteAddress + "GetCommonFileInfo", 'POST', this.sortType);
        return await response.json();
    }

    async loadFile(file) {
        let formData = new FormData();
        formData.append('files', file);
        await FileLoader._sendFile(formData, siteAddress + "LoadFiles");
    }

    async loadCommonFile(file) {
        let formData = new FormData();
        formData.append('files', file);
        await FileLoader._sendCommonFile(formData, siteAddress + "LoadCommonFiles");
    }

    static async _sendFile(formData, url) {
        let init = {
            method: 'POST',
            body: formData
        };
        await fetch(url, init);
        updatePage();
    }

    static async _sendCommonFile(formData, url) {
        let init = {
            method: 'POST',
            headers : {
                'GroupName': FileLoader.groupLogin.Name,
                'GroupPassword': FileLoader.groupLogin.GroupPassword
            },
            body: formData
        };
        await fetch(url, init);
        updatePage();
    }

    showFilesList(json) {
        let file = document.createElement("div");
        file.className = "file";
        file.classList.add("file-list")
        file.innerHTML = `<div class=\"dropdown-content\">\n` +
            `<p id=\"load-this-${json.name}\">Скачать</p>\n` +
            `<p id=\"delete-this-${json.name}\">Удалить</p>\n` +
            `</div>` +
            `<div class=\"file-name\">${json.name}</div>\n`;
        return file;
    }

    showFilesTable(json) {
        let file = document.createElement("div");
        file.className = "file";
        file.innerHTML = `<div class=\"dropdown-content\">\n` +
            `<p id=\"load-this-${json.name}\">Скачать</p>\n` +
            `<p id=\"delete-this-${json.name}\">Удалить</p>\n` +
            `</div>` +
            `<img  alt=\"\" src=\"${siteAddress}images/free-icon-file-149345.png\" class=\"file-img\"/>\n` +
            `<div class=\"file-name\">${FileLoader._createCurrentFileName(json.name, json.typeOfFile)}</div>\n`;
        return file;
    }

    static _createCurrentFileName(fileName, typeOfFile) {

        if (fileName.length > 20) {
            let currentFileName = "";

            for (let i = 0; i < 20 - typeOfFile.length; i++) {
                currentFileName += fileName[i];
            }
            currentFileName += ".." + typeOfFile;

            fileName = currentFileName;
        }
        if (fileName.length > 12) {
            fileName = fileName.slice(0, 11) + "\n" + fileName.slice(11, fileName.length);
        }

        return fileName;
    }

    addClickEvent(content) {
        let loadButton = document.getElementById(`load-this-${content.name}`);
        let deleteButton = document.getElementById(`delete-this-${content.name}`);
        loadButton.addEventListener("click", () => FileLoader._loadFileByClick(content.name).then());
        deleteButton.addEventListener("click", () => FileLoader._deleteOneFile(content.name).then());
    }

    static async _loadFileByClick(name) {
        let response = await sendJsonMessage(siteAddress + "GetVirtualFile", 'POST', name)
        let blob = await response.blob();
        let url = await URL.createObjectURL(blob);

        const dummy = document.createElement('a');
        dummy.href = url;
        dummy.download = name;
        document.body.appendChild(dummy);
        dummy.click();
    }

    static async _deleteOneFile(name) {
        if (confirm("Вы уверены, что хотите удалить файл?")) {
            await sendJsonMessage(siteAddress + "DeleteOneFile", 'DELETE', name);
            updatePage();
        }
    }

    addCommonClickEvent(content) {
        let loadButton = document.getElementById(`load-this-${content.name}`);
        let deleteButton = document.getElementById(`delete-this-${content.name}`);
        loadButton.addEventListener("click", () => FileLoader._loadCommonFileByClick(content.name).then());
        deleteButton.addEventListener("click", () => FileLoader._deleteOneCommonFile(content.name).then());
    }

    static async _loadCommonFileByClick(name) {
        let response = await sendCommonJsonMessage(siteAddress + "GetCommonVirtualFile", 'POST', name)
        let blob = await response.blob();
        let url = await URL.createObjectURL(blob);

        const dummy = document.createElement('a');
        dummy.href = url;
        dummy.download = name;
        document.body.appendChild(dummy);
        dummy.click();
    }

    static async _deleteOneCommonFile(name) {
        if (confirm("Вы уверены, что хотите удалить файл?")) {
            await sendCommonJsonMessage(siteAddress + "DeleteOneCommonFile", 'DELETE', name);
            updatePage();
        }
    }
}