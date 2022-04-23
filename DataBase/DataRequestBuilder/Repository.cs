using MyCloud.DataBase.Interfaces;

namespace MyCloud.DataBase.DataRequestBuilder
{
    public class Repository
    {
        public IFilesRepository FilesRepository { get; set; }
        public IGroupsRepository GroupsRepository { get; set; }
        public IPersonalityRepository PersonalityRepository { get; set; }
        public IUsersRepository UsersRepository { get; set; }
        public IFilesRepository CommonFilesRepository { get; set; }
        public IRegistrationRepository RegistrationRepository { get; set; }
    }
}