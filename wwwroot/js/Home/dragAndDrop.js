function handleDrop(e) {
    if (pageUpdater.typeOfPage === "groups") return;
    let dt = e.dataTransfer;
    let files = dt.files;
    ([...files]).forEach(function (file) {
        pageUpdater.loadDragAndDropFile(file).then();
    });
}

function highlight(container) {
    if (pageUpdater.typeOfPage === "groups") return;
    container.classList.add('highlight');
}

function unhighlight(container) {
    if (pageUpdater.typeOfPage === "groups") return;
    container.classList.remove('highlight');
}

function preventDefaults (e) {
    if (pageUpdater.typeOfPage === "groups") return;
    e.preventDefault();
    e.stopPropagation();
}