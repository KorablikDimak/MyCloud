function handleDrop(e) {
    let dt = e.dataTransfer;
    let files = dt.files;
    ([...files]).forEach(uploadFile);
}

function highlight(container) {
    container.classList.add('highlight');
}

function unhighlight(container) {
    container.classList.remove('highlight');
}

function preventDefaults (e) {
    e.preventDefault();
    e.stopPropagation();
}

