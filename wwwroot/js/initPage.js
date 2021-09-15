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
        container.addEventListener(eventName, ev => highlight(container), false)
    });

    ;['dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, ev => unhighlight(container), false)
    });

    container.addEventListener('drop', handleDrop, false)

    updatePage().then();
}