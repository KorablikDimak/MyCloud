using MyCloud.DataBase.Interfaces;

namespace MyCloud.DataBase.DataRequestBuilder
{
    public interface IDatabaseRequest
    {
        public IDatabaseFilesRequest DatabaseFilesRequest { get; set; }
        public IDatabaseGroupsRequest DatabaseGroupsRequest { get; set; }
        public IDatabasePersonalityRequest DatabasePersonalityRequest { get; set; }
        public IDatabaseUsersRequest DatabaseUsersRequest { get; set; }
        public IDatabaseFilesRequest DatabaseCommonFilesRequest { get; set; }
    }
}