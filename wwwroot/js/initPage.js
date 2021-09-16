initPage();

function initPage(){
    let loadInput = document.getElementById("load-file");
    let deleter = document.getElementById("delete-all-files")
    let container = document.getElementById("container");

    loadInput.addEventListener("change", ev => loadInputChanged(loadInput));
    deleter.addEventListener("click", ev => deleteAllFiles());
    
    let sortBy = [document.getElementById("name"), 
        document.getElementById("typeoffile"), 
        document.getElementById("datetime"), 
        document.getElementById("size")];
    
    let ASC = document.getElementById("ASC");
    let DESC = document.getElementById("DESC");

    let currentSortType = document.getElementById("current-sort-type");
    let arrow = document.getElementById("arrow");
    
    ASC.addEventListener("click", ev => {
        fileInfoBody.typeOfSort = "ASC";
        arrow.src = "images/free-icon-down-arrow-134210.png";
        updatePage().then();
    });
    
    DESC.addEventListener("click", ev => {
        fileInfoBody.typeOfSort = "DESC";
        arrow.src = "images/free-icon-up-arrow-134211.png";
        updatePage().then();
    });

    sortBy.forEach(addClickEvent);

    function addClickEvent(sortBy) {
        sortBy.addEventListener("click", ev => {
            fileInfoBody.orderBy = sortBy.id;
            currentSortType.innerText = sortBy.innerText;
            updatePage().then();
        });
    }

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