initPage();

function initPage() {
    let loadInput = document.getElementById("load-file");
    let loadForm = document.getElementById("load-file-form");
    let deleter = document.getElementById("delete-all-files")
    
    loadInput.addEventListener("change", ev => loadInputChanged(loadInput, loadForm));
    deleter.addEventListener("click", ev => deleteAllFiles())
    
    clearFileContainer();
    updateFileContainer();
}

function clearFileContainer() {
    let fileContainer = document.getElementById("files");
    fileContainer.innerHTML = "";
}

async function loadInputChanged(loadInput, loadForm) {
    if (loadInput.files.length <= 10){
        let sizeOfFiles = 0;

        for (let i = 0; i < loadInput.files.length; i++){
            sizeOfFiles += loadInput.files[i].size;
        }

        if (sizeOfFiles < 1024 * 1024 * 1024){
            loadForm.submit();
        }
    }
}

async function loadFileInfo() {
    const init = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    }
    const response = await fetch("https://localhost:5001/GetFileInfo", init);
    return await response.json();
}

function updateFileContainer() {
    loadFileInfo().then((json) => {
        let fileContainer = document.getElementById("files");
        for (let i = 0; i < json.length; i++){
            const name = json[i].name;
            const typeOfFile = json[i].typeOfFile;

            let file = document.createElement("div");

            file.className = "file";
            file.innerHTML =
                `<div class=\"dropdown-content\">\n` +
                `<p id=\"load-this-${name}\">Скачать</p>\n` +
                `<p id=\"delete-this-${name}\">Удалить</p>\n` +
                `</div>` +
                `<img  alt=\"\" src=\"images/free-icon-file-149345.png\" class=\"file-img\"/>\n` +
                `<div class=\"file-name\">${createCurrentFileName(name, typeOfFile)}</div>\n`;
            fileContainer.append(file);

            updateButtons(name);
        }
    });
}

function updateButtons(name) {
    let loadButton = document.getElementById(`load-this-${name}`);
    let deleteButton = document.getElementById(`delete-this-${name}`);
    
    loadButton.addEventListener("click", ev => loadFileByClick(name));
    deleteButton.addEventListener("click", ev => deleteOneFile(name));
}

function createCurrentFileName(fileName, typeOfFile) {
    if (fileName.length > 20){
        let currentFileName = "";
        
        for (let i = 0; i < 15; i++){
            currentFileName+= fileName[i];
        }
        currentFileName+= ".." + typeOfFile;
        return currentFileName;
    }
    return fileName;
}

async function loadFileByClick(name){
    const init = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(name)
    }
    
    let response = await fetch("https://localhost:5001/GetFile", init);
    let blob = await response.blob();
    let url = await URL.createObjectURL(blob);

    const dummy = document.createElement('a');
    dummy.href = url;
    dummy.download = name;
    document.body.appendChild(dummy);
    dummy.click();
    clearFileContainer();
    updateFileContainer();
}

async function deleteOneFile(name) {
    const init = {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(name)
    }
    await fetch("https://localhost:5001/DeleteOneFile", init);
    clearFileContainer();
    updateFileContainer();
}

async function deleteAllFiles(){
    const init = {
        method: 'DELETE'
    }
    await fetch("https://localhost:5001/DeleteAllFiles", init);
    clearFileContainer();
    updateFileContainer();
}