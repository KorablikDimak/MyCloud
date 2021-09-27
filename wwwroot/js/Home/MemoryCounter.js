class MemoryCounter {
    url = "https://localhost:5001/GetMemorySize";
    _freeSize;
    
    async showFreeMemory() {
        let memorySize = await this.getMemorySize();
        this._freeSize = 10240 - Math.round(memorySize / 1024 / 1024);
        this._updateMemoryText();
        this._updateMemoryIndicator();
    }

    _updateMemoryText() {
        let memoryText = document.getElementById("free-size-of-memory");
        memoryText.innerText = `доступно ${this._freeSize} Мбайт из ${10240}`;
    }

    _updateMemoryIndicator() {
        let percent = 100 - (100 * this._freeSize / 10240);
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