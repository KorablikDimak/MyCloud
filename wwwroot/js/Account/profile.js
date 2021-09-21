let changePhoto = document.getElementById("change-photo");
changePhoto.addEventListener("click", ev => {
    //TODO open manager for photo
});

let changeData = document.getElementById("change-data");
changeData.addEventListener("click", ev => {
    //TODO change data
});

let myFiles = document.getElementById("my-files");
myFiles.addEventListener("click", ev => {
    window.location = "https://localhost:5001/Home/MyFiles";
});

let groupsView = document.getElementById("groups-view");
groupsView.addEventListener("click", ev => {
    //TODO groups
});

let changePassword = document.getElementById("password-change");
changePassword.addEventListener("click", ev => {
    //TODO change password
});

let deleteAccount = document.getElementById("delete-account");
deleteAccount.addEventListener("click", ev => {
    //TODO delete account
});