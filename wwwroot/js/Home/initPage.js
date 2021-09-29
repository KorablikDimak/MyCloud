let pageUpdater = new PageUpdater();
pageUpdater.updatePage().then();
pageUpdater.addSearch();

function updatePage() {
    pageUpdater.updatePage().then();
}

let header = document.getElementById("file-header");

let commonButton = document.getElementById("common-files");
commonButton.addEventListener("click", () => {
    pageUpdater.typeOfPage = "groups";
    header.innerText = "Мои группы";
    updatePage();
});

let myFilesButton = document.getElementById("my-files");
myFilesButton.addEventListener("click", () => {
    pageUpdater.typeOfPage = "myFiles";
    header.innerText = "Мои файлы";
    updatePage();
});

let profileButton = document.getElementById("profile");
profileButton.addEventListener("click", () => {
    window.location = "Https://192.168.1.130/Account/Profile"
});

let logOutButton = document.getElementById("logOut-button");
logOutButton.addEventListener("click", () => {
    logout().then();
});

let loadInput = document.getElementById("load-file");
loadInput.addEventListener("change", () => pageUpdater.loadFiles(loadInput));

let deleter = document.getElementById("delete-all-files")
deleter.addEventListener("click", () => pageUpdater.deleteAll());

let tableButton = document.querySelector(".icon-table-container");
tableButton.style.backgroundColor = "#e7e7e7";
tableButton.addEventListener("click", () => {
    tableButton.style.backgroundColor = "#e7e7e7";
    listButton.style.backgroundColor = "rgba(56,56,56,0)";
    pageUpdater.showType = "table";
    updatePage();
});

let listButton = document.querySelector(".icon-list-container");
listButton.addEventListener("click", () => {
    listButton.style.backgroundColor = "#e7e7e7";
    tableButton.style.backgroundColor = "rgba(56,56,56,0)";
    pageUpdater.showType = "list";
    updatePage();
});

let container = document.getElementById("container");
;['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    container.addEventListener(eventName, preventDefaults, false);
});
;['dragenter', 'dragover'].forEach(eventName => {
    container.addEventListener(eventName, () => highlight(container), false);
});
;['dragleave', 'drop'].forEach(eventName => {
    container.addEventListener(eventName, () => unhighlight(container), false);
});
container.addEventListener('drop', handleDrop, false);

let sortType = {
    OrderBy: "name",
    TypeOfSort: "ASC"
}

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

ASC.addEventListener("click", () => {
    sortType.TypeOfSort = "ASC"
    pageUpdater.changeSortType(sortType);
    arrow.src = "https://192.168.1.130/images/free-icon-down-arrow-134210.png";
    updatePage();
});

DESC.addEventListener("click", () => {
    sortType.TypeOfSort = "DESC";
    pageUpdater.changeSortType(sortType);
    arrow.src = "https://192.168.1.130/images/free-icon-up-arrow-134211.png";
    updatePage();
});

sortBy.forEach(addClickEvent);

function addClickEvent(sortBy) {
    sortBy.addEventListener("click", () => {
        sortType.OrderBy = sortBy.id;
        pageUpdater.changeSortType(sortType);
        currentSortType.innerText = sortBy.innerText;
        updatePage();
    });
}

function sortOptionClicked(dropDownLine) {
    dropDownLine.addEventListener("click", () => {
        let otherChosen = document.querySelectorAll(`.${dropDownLine.classList[1].toString()}` + " > .chosen");
        otherChosen.forEach(function (other) {
            other.style.backgroundColor = "rgba(56,56,56,0)";
        });
        let chosen = dropDownLine.querySelector(".chosen");
        chosen.style.backgroundColor = "#404040";
    });
}