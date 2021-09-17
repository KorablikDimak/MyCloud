let fileInfoBody = {
    orderBy: "name",
    typeOfSort: "ASC"
}

let showType = showFileTable;

async function updatePage() {
    clearFileContainer();
    await updateFileContainer();
    await showFreeMemory();
}

function clearFileContainer() {
    let fileContainer = document.getElementById("files");
    fileContainer.innerHTML = "";
}

async function loadFileInfo() {
    const init = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            orderBy: fileInfoBody.orderBy,
            typeOfSort: fileInfoBody.typeOfSort
        })
    }
    const response = await fetch("https://localhost:5001/GetFileInfo", init);
    return await response.json();
}

async function updateFileContainer() {
    let json = await loadFileInfo();
    pullFileContainer(json);
}

function pullFileContainer(json) {
    let fileContainer = document.getElementById("files");
    for (let i = 0; i < json.length; i++) {
        const name = json[i].name;
        const typeOfFile = json[i].typeOfFile;
        fileContainer.append(showType(name, typeOfFile));
        updateButtons(name);
    }
}

function showFileList(name, typeOfFile) {
    let file = document.createElement("div");
    file.className = "file";
    file.classList.add("file-list")
    file.innerHTML = `<div class=\"dropdown-content\">\n` +
        `<p id=\"load-this-${name}\">Скачать</p>\n` +
        `<p id=\"delete-this-${name}\">Удалить</p>\n` +
        `</div>` +
        `<div class=\"file-name\">${name}</div>\n`;
    return file;
}

function showFileTable(name, typeOfFile) {
    let file = document.createElement("div");
    file.className = "file";
    file.innerHTML = `<div class=\"dropdown-content\">\n` +
        `<p id=\"load-this-${name}\">Скачать</p>\n` +
        `<p id=\"delete-this-${name}\">Удалить</p>\n` +
        `</div>` +
        `<img  alt=\"\" src=\"images/free-icon-file-149345.png\" class=\"file-img\"/>\n` +
        `<div class=\"file-name\">${createCurrentFileName(name, typeOfFile)}</div>\n`;
    return file;
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

        for (let i = 0; i < 20 - typeOfFile.length; i++){
            currentFileName += fileName[i];
        }
        currentFileName += ".." + typeOfFile;
        
        fileName = currentFileName;
    }
    if (fileName.length > 12){
        fileName = fileName.slice(0, 11) + "\n" + fileName.slice(11, fileName.length - 1);
    }
    
    return fileName;
}

async function showFreeMemory() {
    let freeSize = 10240 - Math.round((await getMemorySize()) / 1024 / 1024);
    updateMemoryText(freeSize);
    updateMemoryIndicator(freeSize);
}

function updateMemoryText(freeSize) {
    let memoryText = document.getElementById("free-size-of-memory");
    memoryText.innerText = `доступно ${freeSize} Мбайт из ${10240}`;
}

function updateMemoryIndicator(freeSize) {
    let percent = 100 - (100 * freeSize / 10240);
    let memoryBar = document.getElementById("memory-bar");
    memoryBar.style.width = (percent) + "%";
    memoryBar.style.backgroundColor = getGradientColor("#5bff76", "#ff1c1c", percent);
    memoryBar.innerHTML = `<p class=\"memory-text-percent\">${percent.toFixed(2)}%</p>`;
}

async function getMemorySize() {
    const init = {
        method: 'GET'
    }
    const response = await fetch("https://localhost:5001/GetMemorySize", init);
    return await response.json();
}