const maxSize = 10240; //Mb
const requestLimit = 1024; //Mb

class MemoryCounter {
    url = siteAddress + "GetMemorySize";
    _freeSize;
    
    async showFreeMemory() {
        let memorySize = await this.getMemorySize();
        this._freeSize = maxSize - Math.round(memorySize / 1024 / 1024);
        this._updateMemoryText();
        this._updateMemoryIndicator();
    }

    _updateMemoryText() {
        let memoryText = document.getElementById("free-size-of-memory");
        memoryText.innerText = `доступно ${this._freeSize} Мбайт из ${maxSize}`;
    }

    _updateMemoryIndicator() {
        let percent = 100 - (100 * this._freeSize / maxSize);
        let memoryBar = document.getElementById("memory-bar");
        memoryBar.style.width = (percent) + "%";
        memoryBar.style.backgroundColor = getGradientColor("#5bff76", "#ff1c1c", percent);
        memoryBar.innerHTML = `<p class=\"memory-text-percent\">${percent.toFixed(2)}%</p>`;
    }

    async getMemorySize() {
        let response = await sendJsonMessage(this.url, 'GET');
        return await response.json();
    }
}