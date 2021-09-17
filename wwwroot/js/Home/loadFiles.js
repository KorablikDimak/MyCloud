async function sendFile(formData) {
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
    if (confirm("Вы уверены, что хотите удалить файл?")) {
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

async function deleteAllFiles() {
    if (confirm("Вы уверены, что хотите удалить все файлы?")) {
        const init = {
            method: 'DELETE'
        }
        await fetch("https://localhost:5001/DeleteAllFiles", init);
        
        updatePage().then();
    }
}