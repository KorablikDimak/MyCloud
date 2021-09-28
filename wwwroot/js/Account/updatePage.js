updatePage().then();

async function updatePage() {
    let response = await sendJsonMessage("https://localhost:5001/GetPersonality", 'GET');
    let json = await response.json();
    
    let surname = document.getElementById("surname");
    let name = document.getElementById("name");
    let username = document.getElementById("username");
    let id = document.getElementById("id");
    
    surname.innerText = json.surname;
    name.innerText = json.name;
    username.innerText = json.userName;
    id.innerText = `Ваш id: ${json.id}`;

    getUserPhoto("photo");
    
    document.getElementById("surname-input").value = json.surname;
    document.getElementById("name-input").value = json.name;
    document.getElementById("username-input").value = json.userName;
}