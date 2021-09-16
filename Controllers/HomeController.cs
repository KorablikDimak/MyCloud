using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.Models.MyFile;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private const string Connect = "Data Source=LAPTOP-KPPKGVU7\\LOVE;Initial Catalog=MyCloud;Persist Security Info=True;User ID=root;Password=Faggot_2002";
            
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [RequestSizeLimit(1073741824)]
        [HttpPost("LoadFile")]
        public async Task<IActionResult> LoadFile(ICollection<IFormFile> files)
        {
            foreach (var file in files) 
            {
                var untrustedFileName = Path.GetFileName(file.FileName);
                var filePath = $"wwwroot\\data\\{untrustedFileName}";
                await using var stream = System.IO.File.Create(filePath);
                if (!IsMemoryFree(stream.Length)) return new ConflictResult();
                
                var fileInfo = new FileInfo(filePath);
                var myFileInfo = new MyFileInfo(fileInfo.Name, fileInfo.Extension, fileInfo.CreationTime, stream.Length);
                
                if (await LoadFileInfoToDataBaseAsync(myFileInfo))
                {
                    await file.CopyToAsync(stream);
                }
                else
                {
                    stream.Close();
                    System.IO.File.Delete(filePath);
                    return new ConflictResult();
                }
            }

            return Ok();
        }

        private static async Task<bool> LoadFileInfoToDataBaseAsync(MyFileInfo fileInfo)
        {
            var connection = new SqlConnection(Connect);
            await using (connection)
            {
                try
                {
                    var commandText = "INSERT INTO Files (name, typeoffile, datetime, size) VALUES " +
                                      $"(N'{fileInfo.Name}', N'{fileInfo.TypeOfFile}', '{fileInfo.DateTimeUpload}', {fileInfo.Size})";
                    await connection.OpenAsync();
                    var command = new SqlCommand(commandText, connection);
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        private static bool IsMemoryFree(long fileSize)
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            return dirInfo.GetFiles().Sum(file => file.Length) + fileSize <= MaxMemorySize;
        }

        [HttpPost("GetFileInfo")]
        public async Task<List<MyFileInfo>> GetFileInfo([FromBody] SortType sortType)
        {
            var fileInfoToSend = new List<MyFileInfo>();

            var connection = new SqlConnection(Connect);
            await using (connection)
            {
                try
                {
                    var commandText = $"SELECT * FROM Files ORDER BY {sortType.OrderBy} {sortType.TypeOfSort}";
                    await connection.OpenAsync();
                    var command = new SqlCommand(commandText, connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var fileInfo = new MyFileInfo(
                            reader.GetInt32(0), 
                            reader.GetString(1), 
                            reader.GetString(2), 
                            reader.GetSqlDateTime(3), 
                            reader.GetInt64(4));
                        fileInfoToSend.Add(fileInfo);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }

            return fileInfoToSend;
        }

        [RequestSizeLimit(1073741824)]
        [HttpPost("GetFile")]
        public VirtualFileResult GetVirtualFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("~/data", fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("wwwroot\\data", fileName);
            if (!await DeleteFileFromDataBaseAsync(fileName)) return new ConflictResult();
            System.IO.File.Delete(filepath);
            return Ok();
        }

        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            foreach (var file in dirInfo.GetFiles())
            {
                if (await DeleteFileFromDataBaseAsync(file.Name))
                {
                    file.Delete();
                }
                else
                {
                    return new ConflictResult();
                }
            }
            
            return Ok();
        }

        private static async Task<bool> DeleteFileFromDataBaseAsync(string name)
        {
            var connection = new SqlConnection(Connect);
            await using (connection)
            {
                try
                {
                    var commandText = $"DELETE FROM Files WHERE name = N'{name}'";
                    await connection.OpenAsync();
                    var command = new SqlCommand(commandText, connection);
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            return dirInfo.GetFiles().Sum(file => file.Length);
        }
    }
}