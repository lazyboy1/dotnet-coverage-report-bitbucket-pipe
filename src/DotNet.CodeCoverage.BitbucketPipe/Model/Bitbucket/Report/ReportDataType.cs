using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.Report
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ReportDataType
    {
        [EnumMember(Value = "BOOLEAN")]
        Boolean,

        [EnumMember(Value = "DATE")]
        Date,

        [EnumMember(Value = "DURATION")]
        Duration,

        [EnumMember(Value = "LINK")]
        Link,

        [EnumMember(Value = "NUMBER")]
        Number,

        [EnumMember(Value = "PERCENTAGE")]
        Percentage,

        [EnumMember(Value = "TEXT")]
        Text
    }
}
