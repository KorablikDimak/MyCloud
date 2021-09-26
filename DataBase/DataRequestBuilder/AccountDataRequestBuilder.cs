namespace MyCloud.DataBase.DataRequestBuilder
{
    public class AccountDataRequestBuilder : DatabaseRequestBuilder
    {
        protected override void BuildFilesRequest()
        {
            //Not uses
        }

        protected override void BuildCommonFilesRequest()
        {
            //Not uses
        }

        protected override void BuildGroupsRequest()
        {
            DatabaseRequest.DatabaseGroupsRequest = new DatabaseGroupsRequest(Context);
        }

        protected override void BuildPersonalityRequest()
        {
            DatabaseRequest.DatabasePersonalityRequest = new DatabasePersonalityRequest(Context);
        }

        protected override void BuildUsersRequest()
        {
            DatabaseRequest.DatabaseUsersRequest = new DatabaseUsersRequest(Context);
        }
    }
}