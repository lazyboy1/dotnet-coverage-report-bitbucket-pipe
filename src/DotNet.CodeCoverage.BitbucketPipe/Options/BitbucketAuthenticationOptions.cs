namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class BitbucketAuthenticationOptions
    {
        public const string BitbucketAuthentication = "BitbucketAuthentication";

        public string Key { get; set; } = null!;
        public string Secret { get; set; } = null!;
    }
}
