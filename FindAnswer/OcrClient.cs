using System.Runtime.InteropServices;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;

namespace FindAnswer
{
    public static class OcrClient
    {
        private static GoogleCredential credential = null;
        private static Grpc.Core.Channel channel = null;
        private static ImageAnnotatorClient client = null;

        public static string Recognize(string imagePath)
        {
            if(credential == null)
            {
                var googleCredsPath = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? @"C:\mydev\My Project-77101559a6d3.json"
                    : "/Users/slav/Downloads/My Project-d1092d64586a.json";

                credential = GoogleCredential.FromFile(googleCredsPath)
                    .CreateScoped(ImageAnnotatorClient.DefaultScopes);
                channel = new Grpc.Core.Channel(
                    ImageAnnotatorClient.DefaultEndpoint.ToString(),
                    credential.ToChannelCredentials());
            
                client = ImageAnnotatorClient.Create(channel);
            }

            var image = Image.FromFile(imagePath);
            var response = client.DetectText(image);
            int count = response.Count;
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                    return annotation.Description;
            }
            return null;
        }
    }
}