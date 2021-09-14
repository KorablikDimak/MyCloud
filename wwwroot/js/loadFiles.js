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
})