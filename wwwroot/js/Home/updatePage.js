let fileInfoBody = {
    orderBy: "name",
    typeOfSort: "ASC"
}

let showType = showFileTable;

function updatePage() {
    clearFileContainer();
    updateFileContainer().then();
    showFreeMemory().then();
}

function clearFileContainer() {
    let fileContainer = document.getElementById("files");
    fileContainer.innerHTML = "";
}

async function loadFileInfo() {
    const message = {
        orderBy: fileInfoBody.orderBy,
        typeOfSort: fileInfoBody.typeOfSort
    }
    let response = await sendJsonMessage("https://localhost:5001/GetFileInfo", 'POST', message);
    return await response.json();
}

async function updateFileContainer() {
    const json = await loadFileInfo();
    pullFileContainer(json);
}

function pullFileContainer(json) {
    let fileContainer = document.getElementById("files");
    for (let i = 0; i < json.length; i++) {
        fileContainer.append(showType(json[i]));
        updateButtons(json[i].name);
    }
}

function showFileList(json) {
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

function showFileTable(json) {
    let file = document.createElement("div");
    file.className = "file";
    file.innerHTML = `<div class=\"dropdown-content\">\n` +
        `<p id=\"load-this-${json.name}\">Скачать</p>\n` +
        `<p id=\"delete-this-${json.name}\">Удалить</p>\n` +
        `</div>` +
        `<img  alt=\"\" src=\"https://localhost:5001/images/free-icon-file-149345.png\" class=\"file-img\"/>\n` +
        `<div class=\"file-name\">${createCurrentFileName(json.name, json.typeOfFile)}</div>\n`;
    return file;
}

function updateButtons(name) {
    let loadButton = document.getElementById(`load-this-${name}`);
    let deleteButton = document.getElementById(`delete-this-${name}`);

    loadButton.addEventListener("click", () => loadFileByClick(name));
    deleteButton.addEventListener("click", () => deleteOneFile(name));
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
        fileName = fileName.slice(0, 11) + "\n" + fileName.slice(11, fileName.length);
    }
    
    return fileName;
}

async function showFreeMemory() {
    let memorySize = await getMemorySize();
    const freeSize = 10240 - Math.round(memorySize / 1024 / 1024);
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
    let response = await sendJsonMessage("https://localhost:5001/GetMemorySize", 'GET');
    return await response.json();
}