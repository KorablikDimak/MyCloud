initPage();

function initPage() {
    let loadInput = document.getElementById("load-file");
    let deleter = document.getElementById("delete-all-files")
    let container = document.getElementById("container");
    
    loadInput.addEventListener("change", ev => loadInputChanged(loadInput));
    deleter.addEventListener("click", ev => deleteAllFiles())
        
    ;['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, preventDefaults, false)
    });

    ;['dragenter', 'dragover'].forEach(eventName => {
        container.addEventListener(eventName, highlight, false)
    });
    
    ;['dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, unhighlight, false)
    });

    container.addEventListener('drop', handleDrop, false)
    
    updatePage().then();
}

function handleDrop(e) {
    let dt = e.dataTransfer;
    let files = dt.files;
    ([...files]).forEach(uploadFile);
}

function highlight(e) {}

function unhighlight(e) {}

function preventDefaults (e) {
    e.preventDefault()
    e.stopPropagation()
}

async function sendFile(formData){
    const init = {
        method: 'POST',
        body: formData
    }
    await fetch("https://localhost:5001/LoadFile", init);
}

async function uploadFile(file) {
    let formData = new FormData();
    formData.append('files', file);
    await sendFile(formData);
    await updatePage();
}

async function loadInputChanged(loadInput) {
    if (loadInput.files.length <= 10){
        let sizeOfFiles = 0;
        for (let i = 0; i < loadInput.files.length; i++){
            sizeOfFiles += loadInput.files[i].size;
        }
        if (sizeOfFiles < 1024 * 1024 * 1024){
            for (let i = 0; i < loadInput.files.length; i++){
                await uploadFile(loadInput.files[i]);
            }
        }
        else {
            window.alert("Общий объем загружаемых файлов не должен превышать 1гб");
        }
    }
    else {
        window.alert("Нельзя отправлять более 10 файлов за раз");
    }
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
    
    updatePage().then();
}

async function deleteOneFile(name) {
    if (confirm("Вы уверены, что хотите удалить файл?")){
        const init = {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(name)
        }
        await fetch("https://localhost:5001/DeleteOneFile", init);
        
        updatePage().then();
    }
}

async function deleteAllFiles(){
    if (confirm("Вы уверены, что хотите удалить все файлы?")) {
        const init = {
            method: 'DELETE'
        }
        await fetch("https://localhost:5001/DeleteAllFiles", init);
        
        updatePage().then();
    }
}