class PageUpdater {
    _searchInput = document.getElementById("search-input");
    
    _groupLoader = new GroupLoader();
    _fileLoader = new FileLoader();
    _container = new Container();
    _memoryCounter = new MemoryCounter();
    
    typeOfPage = "myFiles";
    showType = "table";
    textToFind = "";
    
    changeSortType(sortType)
    {
        this._fileLoader.sortType = sortType;
    }
    
    async loadDragAndDropFile(file) {
        if (this.typeOfPage === "commonFiles") {
            await this._fileLoader.loadCommonFile(file);
        }
        else if (this.typeOfPage === "myFiles") {
            await this._fileLoader.loadFile(file);
        }
    }
    
    async deleteAll() {
        if (this.typeOfPage === "groups") return;
        
        if (confirm("Вы уверены, что хотите удалить все файлы?")) {
            if (this.typeOfPage === "commonFiles") {
                await sendCommonJsonMessage("https://192.168.1.130/DeleteAllCommonFiles", 'DELETE');
            }
            else if (this.typeOfPage === "myFiles") {
                await sendJsonMessage("https://192.168.1.130/DeleteAllFiles", 'DELETE');
            }
            updatePage();
        }
    }
    
    async loadFiles(loadInput) {
        if (this.typeOfPage === "commonFiles") {
            this._memoryCounter.url = "https://192.168.1.130/GetCommonMemorySize";
            await this._loadFiles(loadInput, this._fileLoader.loadCommonFile);
        }
        else if (this.typeOfPage === "myFiles") {
            this._memoryCounter.url = "https://192.168.1.130/GetMemorySize";
            await this._loadFiles(loadInput, this._fileLoader.loadFile);
        }
    }
    
    async _loadFiles(loadInput, loadFile) {
        let sizeOfFiles = 0;
        for (let i = 0; i < loadInput.files.length; i++) {
            sizeOfFiles += loadInput.files[i].size;
        }
        if (sizeOfFiles < 1073741824){
            if (10737418240 - await this._memoryCounter.getMemorySize() >= sizeOfFiles) {
                for (let i = 0; i < loadInput.files.length; i++) {
                    await loadFile(loadInput.files[i]);
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
    
    addSearch() {
        this._searchInput.addEventListener("input", () => {
            this.textToFind = this._searchInput.value.toLowerCase();
            this.updatePage().then();
        });
    }
    
    async updatePage() {
        this._container.clearContainer();
        this._memoryCounter.url = "https://192.168.1.130/GetCommonMemorySize";
        if (this.typeOfPage === "commonFiles") {
            await this._updateCommonFiles();
        }
        else if (this.typeOfPage === "groups") {
            await this._updateGroups();
        }
        else {
            this._memoryCounter.url = "https://192.168.1.130/GetMemorySize";
            await this._updateMyFiles();
        }
        this._memoryCounter.showFreeMemory().then();
        this._container.pullContainer();
        getUserPhoto("user-photo");
    }
    
     _createCurrentContent(content) {
        let textToFind = this.textToFind;
        if (textToFind !== "") {
            let currentContent = [];
            content.forEach(function (content) {
                if (content.name.toLowerCase().indexOf(textToFind) !== -1) {
                    currentContent.push(content);
                }
            });
            this._container.content = currentContent;
        }
        else {
            this._container.content = content;
        }
    }
    
    async _updateMyFiles() {
        let content = await this._fileLoader.getFileInfo();
        this._createCurrentContent(content);
        if (this.showType === "list") {
            this._container.showType = this._fileLoader.showFilesList;
        }
        else {
            this._container.showType = this._fileLoader.showFilesTable;
        }
        this._container.clickEvent = this._fileLoader.addClickEvent;
    }
    
    async _updateCommonFiles() {
        let content = await this._fileLoader.getCommonFileInfo();
        this._createCurrentContent(content);
        if (this.showType === "list") {
            this._container.showType = this._fileLoader.showFilesList;
        }
        else {
            this._container.showType = this._fileLoader.showFilesTable;
        }
        this._container.clickEvent = this._fileLoader.addCommonClickEvent;
    }
    
    async _updateGroups() {
        let content = await this._groupLoader.findMyGroups();
        this._createCurrentContent(content);
        if (this.showType === "list") {
            this._container.showType = this._groupLoader.showGroupsList;
        }
        else {
            this._container.showType = this._groupLoader.showGroupsTable;
        }
        this._container.clickEvent = this._groupLoader.addClickEvent;
    }
}