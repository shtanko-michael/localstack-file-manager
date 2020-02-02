namespace LocalStack {
    public class AppSettings {
        public AwsSettings Aws { get; set; }
        public string ClientUrl { get; set; }
    }

    public class AwsSettings {
        public string Region { get; set; }
        public AwsS3Settings S3 { get; set; }
        public AwsCloudFrontSettings CloudFront { get; set; }
        public AwsCloudWatchSettings CloudWatch { get; set; }
    }

    public class AwsS3Settings {
        public string ServiceUrl { get; set; }
        public string Bucket { get; set; }
    }

    public class AwsCloudFrontSettings {
        public string Url { get; set; }
    }

    public class AwsCloudWatchSettings {
        public string ServiceUrl { get; set; }
        public string GroupName { get; set; }
    }
}
