namespace MyCloud.DataBase.DataRequestBuilder
{
    public class HomeDataRequestBuilder : DatabaseRequestBuilder
    {
        protected override void BuildFilesRequest()
        {
            DatabaseRequest.DatabaseFilesRequest = new DatabaseFilesRequest(Context);
        }

        protected override void BuildCommonFilesRequest()
        {
            DatabaseRequest.DatabaseCommonFilesRequest = new DatabaseCommonFilesRequest(Context);
        }

        protected override void BuildGroupsRequest()
        {
            DatabaseRequest.DatabaseGroupsRequest = new DatabaseGroupsRequest(Context);
        }

        protected override void BuildPersonalityRequest()
        {
            //Not uses
        }

        protected override void BuildUsersRequest()
        {
            DatabaseRequest.DatabaseUsersRequest = new DatabaseUsersRequest(Context);
        }
    }
}