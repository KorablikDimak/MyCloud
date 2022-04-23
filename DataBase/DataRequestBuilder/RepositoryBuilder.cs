using MyCloud.DataBase.Interfaces;

namespace MyCloud.DataBase.DataRequestBuilder
{
    public class RepositoryBuilder : IRepositoryBuilder
    {
        public Repository Repository { get; private set; }
        public DataContext Context { private get; init; }
        
        public void CreateRepository()
        {
            Repository = new Repository();
            BuildFilesRepository();
            BuildCommonFilesRepository();
            BuildGroupsRepository();
            BuildPersonalityRepository();
            BuildUsersRepository();
            BuildRegistrationRepository();
        }

        private void BuildFilesRepository()
        {
            Repository.FilesRepository = new FilesDatabaseRequest(Context);
        }

        private void BuildCommonFilesRepository()
        {
            Repository.CommonFilesRepository = new CommonFilesDatabaseRequest(Context);
        }

        private void BuildGroupsRepository()
        {
            Repository.GroupsRepository = new GroupsDatabaseRequest(Context);
        }

        private void BuildPersonalityRepository()
        {
            Repository.PersonalityRepository = new PersonalityDatabaseRequest(Context);
        }

        private void BuildUsersRepository()
        {
            Repository.UsersRepository = new UsersDatabaseRequest(Context);
        }

        private void BuildRegistrationRepository()
        {
            Repository.RegistrationRepository = new RegistrationDatabaseRequest(Context);
        }
    }
}