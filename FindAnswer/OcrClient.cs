﻿using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;

namespace FindAnswer
{
    public static class OcrClient
    {
        public static string Recognize(string imagePath)
        {
            var credential = GoogleCredential.FromFile(@"C:\mydev\My Project-77101559a6d3.json")
                .CreateScoped(ImageAnnotatorClient.DefaultScopes);
            var channel = new Grpc.Core.Channel(
                ImageAnnotatorClient.DefaultEndpoint.ToString(),
                credential.ToChannelCredentials());
            
            var client = ImageAnnotatorClient.Create(channel);
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
