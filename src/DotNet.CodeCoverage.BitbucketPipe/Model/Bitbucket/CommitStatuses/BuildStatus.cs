using System;
using System.Text.Json.Serialization;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.CommitStatuses
{
    [Serializable]
    public class BuildStatus
    {
        public BuildStatus(string key, string name, State state, string workspace, string repoSlug, Uri? url = null)
        {
            Key = key;
            Name = name;
            State = state;
            Url = url ?? new Uri($"https://bitbucket.org/{workspace}/{repoSlug}");
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public State State { get; set; }
        public string? Description { get; set; }
        public Uri Url { get; set; }

        // ReSharper disable once StringLiteralTypo
        [JsonPropertyName("refname")]
        public string? RefName { get; set; }
    }
}
