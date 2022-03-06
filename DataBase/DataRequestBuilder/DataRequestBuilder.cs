using MyCloud.DataBase.Interfaces;

namespace MyCloud.DataBase.DataRequestBuilder
{
    public class DataRequestBuilder : IDatabaseRequestBuilder
    {
        public DatabaseRequest DatabaseRequest { get; private set; }
        public DataContext Context { private get; init; }
        
        public void CreateDatabaseRequest()
        {
            DatabaseRequest = new DatabaseRequest();
            BuildFilesRequest();
            BuildCommonFilesRequest();
            BuildGroupsRequest();
            BuildPersonalityRequest();
            BuildUsersRequest();
        }

        private void BuildFilesRequest()
        {
            DatabaseRequest.DatabaseFilesRequest = new DatabaseFilesRequest(Context);
        }

        private void BuildCommonFilesRequest()
        {
            DatabaseRequest.DatabaseCommonFilesRequest = new DatabaseCommonFilesRequest(Context);
        }

        private void BuildGroupsRequest()
        {
            DatabaseRequest.DatabaseGroupsRequest = new DatabaseGroupsRequest(Context);
        }

        private void BuildPersonalityRequest()
        {
            DatabaseRequest.DatabasePersonalityRequest = new DatabasePersonalityRequest(Context);
        }

        private void BuildUsersRequest()
        {
            DatabaseRequest.DatabaseUsersRequest = new DatabaseUsersRequest(Context);
        }
    }
}