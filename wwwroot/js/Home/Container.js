class Container {
    _container = document.getElementById("files");
    
    content;
    showType;
    clickEvent;
    
    pullContainer() {
        for (let i = 0; i < this.content.length; i++) {
            this._appendElement(this.content[i]);
        }
    }
    
    clearContainer() {
        this._container.innerHTML = "";
    }
    
    _appendElement(content) {
        let element = this.showType(content);
        this._container.append(element);
        this.clickEvent(content);
    }
}