let loadForm = document.getElementById("load-file-form");
let loadInput = document.getElementById("load-file");
loadInput.addEventListener("change", ev => {
    if (loadInput.files.length <= 10){
        let sizeOfFiles = 0;
        for (let i = 0; i < loadInput.files.length; i++){
            sizeOfFiles += loadInput.files[i].size;
        }
        if (sizeOfFiles < 1024 * 1024 * 1024){
            loadForm.submit();
        }
    }
});

async function loadFileInfo() {
    let init = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    }
    const response = await fetch("https://localhost:5001/GetFileInfo", init);
    return await response.json();
}

loadFileInfo().then((json) => {
    let fileContainer = document.getElementById("files");
    for (let i = 0; i < json.length; i++){
        let name = json[i].name;
        let typeOfFile = json[i].typeOfFile;
        let file = document.createElement("div");
        file.className = "file";
        file.innerHTML = `<img  alt=\"\" src=\"images/free-icon-file-149345.png\" class=\"file-img\"/>\n` +
            `<div class=\"file-name\">${createCurrentFileName(name, typeOfFile)}</div>\n`;
        fileContainer.append(file);
    }
});

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