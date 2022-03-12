# MyCloud
![ASP.NET](https://img.shields.io/badge/ASP.NET%20Core-5.0-informational)
![MVC](https://img.shields.io/badge/MVC%20Framework-5.0-informational)
![Entity](https://img.shields.io/badge/Entity%20Framework-6.0-brightgreen)
![InfoLog](https://img.shields.io/badge/InfoLog-1.0.0-orange)
## Description
Remote file sharing service that allows you to quickly download, save and group files. 
There is a fairly large api for possible integration with desktop applications and applications on android / ios.
### Back-end
Executed entirely in **C#** using frameworks `ASP.NET Core MVC` and `Entity Framework`.
There is support for **authorization** based on cookies, **validation** of input data, saving data to a database `MS Sql`.
The application makes extensive use of **asynchrony** for optimal use of machine resources.
Most of the classes are implemented through interfaces, the **builder pattern** was also used, so that individual application modules can be replaced without serious problems with other modules with a different implementation.
### Front-end
Performed on a mixture of **javascript html** and **css**. 
It also supports client-side validation. Many actions do not require contacting the server.
**Asynchrony** is involved so that the user interface is not blocked during requests.
## Peculiarities
![screenMainPage](https://github.com/KorablikDimak/MyCloud/raw/master/screenMainPage.png)
- User authorization
  - each user has his own unique user name
  - each user logs in with their own password and can only have access to their own files
  - you can change personal information on your profile page
  - you can modify a user photo by uploading images from your computer
- Creation of groups
  - users can create their own groups to share shared files with other people
  - users can multiple groups for more convenient sharing of shared files
  - in the group viewing window, you can change the group information as well as view the list of participants
- Uploading files
  - upload files in the usual way at the click of a button
  - download with `drag and drop`
  - delete individual files or download them to your computer
## How to run the application on your computer
- add the line `"ConnectionStrings": {"DefaultConnection": "Data Source=here must be connection string to your database"}` in `appsettings.json`
- if you run the application not on `localhost:5001` then you need to change const `siteAddress` in `wwwroot/js/sendMessage.js` to the site address you need
- for greater security, I recommend making the directory `UserFiles` virtual
- by default, there is a limit on the amount of uploaded files of 1024 MB and on the total disk space of each user at 10240 MB. You can change this by changing the constants `maxSize` and `requestLimit` in `wwwroot/js/Home/MemoryCounter.js` and constans `MaxMemorySize` and `RequestSizeLimit` in `Controllers/HomeController.cs`
## Integration with desktop application
We are planning to write an application using `WPF` technology using an existing api. 
It looks good to use the `git` version control system for the subsequent uploading of repositories to the site.
