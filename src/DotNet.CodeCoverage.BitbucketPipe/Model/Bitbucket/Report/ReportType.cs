using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.Report
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ReportType
    {
        [EnumMember(Value = "SECURITY")]
        Security,

        [EnumMember(Value = "COVERAGE")]
        Coverage,

        [EnumMember(Value = "TEST")]
        Test,

        [EnumMember(Value = "BUG")]
        Bug
    }
}
