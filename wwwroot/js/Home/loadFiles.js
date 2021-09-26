async function sendFile(formData) {
    const init = {
        method: 'POST',
        body: formData
    }
    await fetch("https://localhost:5001/LoadFiles", init);
}

async function uploadFile(file) {
    let formData = new FormData();
    formData.append('files', file);
    await sendFile(formData);
    await updatePage();
}

async function loadInputChanged(loadInput) {
    let sizeOfFiles = 0;
    for (let i = 0; i < loadInput.files.length; i++) {
        sizeOfFiles += loadInput.files[i].size;
    }
    if (sizeOfFiles < 1073741824){
        if (10737418240 - await getMemorySize() >= sizeOfFiles) {
            for (let i = 0; i < loadInput.files.length; i++) {
                await uploadFile(loadInput.files[i]);
            }
        }
        else {
            window.alert("Объем файлов превышает свободное место на диске");
        }
    } 
    else {
        window.alert("Общий объем загружаемых файлов не должен превышать 1гб");
    }
}

async function loadFileByClick(name) {
    let response = await sendJsonMessage("https://localhost:5001/GetFile", 'POST', name)
    let blob = await response.blob();
    let url = await URL.createObjectURL(blob);

    const dummy = document.createElement('a');
    dummy.href = url;
    dummy.download = name;
    document.body.appendChild(dummy);
    dummy.click();
    
    updatePage();
}

async function deleteOneFile(name) {
    if (confirm("Вы уверены, что хотите удалить файл?")) {
        await sendJsonMessage("https://localhost:5001/DeleteOneFile", 'DELETE', name)
        updatePage();
    }
}

async function deleteAllFiles() {
    if (confirm("Вы уверены, что хотите удалить все файлы?")) {
        await sendJsonMessage("https://localhost:5001/DeleteAllFiles", 'DELETE')
        updatePage();
    }
}