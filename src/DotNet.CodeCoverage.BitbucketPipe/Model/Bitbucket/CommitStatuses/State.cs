using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.CommitStatuses
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum State
    {
        [EnumMember(Value = "SUCCESSFUL")]
        Successful,

        [EnumMember(Value = "FAILED")]
        Failed,

        [EnumMember(Value = "INPROGRESS")]
        Inprogress,

        [EnumMember(Value = "STOPPED")]
        Stopped
    }
}