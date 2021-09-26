namespace MyCloud.DataBase.DataRequestBuilder
{
    public abstract class DatabaseRequestBuilder
    {
        public IDatabaseRequest DatabaseRequest { get; private set; }
        public DataContext Context { protected get; init; }

        public void CreateDatabaseRequest()
        {
            DatabaseRequest = new DatabaseRequest();
            BuildFilesRequest();
            BuildCommonFilesRequest();
            BuildGroupsRequest();
            BuildPersonalityRequest();
            BuildUsersRequest();
        }

        protected abstract void BuildFilesRequest();
        protected abstract void BuildCommonFilesRequest();
        protected abstract void BuildGroupsRequest();
        protected abstract void BuildPersonalityRequest();
        protected abstract void BuildUsersRequest();
    }
}