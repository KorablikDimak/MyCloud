initPage();

function initPage() {
    initLoader();
    initDeleter();
    initDragDrop();
    initSortOptions();
    initShowTypeButtons();
    initSearching();
    initLogOutButton().then();
    updatePage().then();
}

async function initLogOutButton() {
    let logOutButton = document.getElementById("logOut-button");
    logOutButton.addEventListener("click", ev => {
        sendJsonMessage("https://localhost:5001/Logout", 'GET').then(r => {
            window.location = "https://localhost:5001/Account/Login";
        });
    });
}

function initSearching() {
    let searchInput = document.getElementById("search-input");
    
    searchInput.addEventListener("input", ev => {
        search(searchInput.value.toLowerCase()).then();
    });
}

async function search(textToFind) {
    let filesInfo = await loadFileInfo();
    let json = [];
    filesInfo.forEach(function (fileInfo) {
        if (fileInfo.name.toLowerCase().indexOf(textToFind) !== -1) {
            json.push(fileInfo);
        }
    });
    clearFileContainer();
    pullFileContainer(json);
}

function initShowTypeButtons() {
    let tableButton = document.querySelector(".icon-table-container");
    tableButton.style.backgroundColor = "#e7e7e7";
    let listButton = document.querySelector(".icon-list-container");
    
    tableButton.addEventListener("click", evt => {
        tableButton.style.backgroundColor = "#e7e7e7";
        listButton.style.backgroundColor = "rgba(56,56,56,0)";
        showType = showFileTable;
        updatePage().then();
    });
    
    listButton.addEventListener("click", evt => {
        listButton.style.backgroundColor = "#e7e7e7";
        tableButton.style.backgroundColor = "rgba(56,56,56,0)";
        showType = showFileList;
        updatePage().then();
    });
}

function initLoader() {
    let loadInput = document.getElementById("load-file");
    loadInput.addEventListener("change", ev => loadInputChanged(loadInput));
}

function initDeleter() {
    let deleter = document.getElementById("delete-all-files")
    deleter.addEventListener("click", ev => deleteAllFiles());
}

function initDragDrop() {
    let container = document.getElementById("container");

    ;['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, preventDefaults, false);
    });

    ;['dragenter', 'dragover'].forEach(eventName => {
        container.addEventListener(eventName, ev => highlight(container), false);
    });

    ;['dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, ev => unhighlight(container), false);
    });

    container.addEventListener('drop', handleDrop, false);
}

function initSortOptions() {
    let sortBy = [document.getElementById("name"),
        document.getElementById("typeoffile"),
        document.getElementById("datetime"),
        document.getElementById("size")];

    document.getElementById("chosen-name").style.backgroundColor = "#404040";
    document.getElementById("chosen-ASC").style.backgroundColor = "#404040";

    let sortOption = document.querySelectorAll(".dropdown-line");
    sortOption.forEach(sortOptionClicked);

    let ASC = document.getElementById("ASC");
    let DESC = document.getElementById("DESC");

    let currentSortType = document.getElementById("current-sort-type");
    let arrow = document.getElementById("arrow");

    ASC.addEventListener("click", ev => {
        fileInfoBody.typeOfSort = "ASC";
        arrow.src = "https://localhost:5001/images/free-icon-down-arrow-134210.png";
        updatePage().then();
    });

    DESC.addEventListener("click", ev => {
        fileInfoBody.typeOfSort = "DESC";
        arrow.src = "https://localhost:5001/images/free-icon-up-arrow-134211.png";
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
}

function sortOptionClicked(dropDownLine) {
    dropDownLine.addEventListener("click", ev => {
        let otherChosen = document.querySelectorAll(`.${dropDownLine.classList[1].toString()}` + " > .chosen");
        otherChosen.forEach(function (other) {
            other.style.backgroundColor = "rgba(56,56,56,0)";
        });
        let chosen = dropDownLine.querySelector(".chosen");
        chosen.style.backgroundColor = "#404040";
    });
}