using System;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Iam.v1;
using Google.Apis.Services;
using Google.Cloud.Storage.V1;

namespace Pixelynx.Logic.Helpers
{
    public static class GCPHelper
    {
        public static async Task<GoogleCredential> GetCredential()
        {
            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            }

            return credential;
        }

        public static async Task<string> GetJwt()
        {
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage serviceAccountRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/identity?audience=http://vault/my-iam-role&format=full"),
                    Headers = { { "Metadata-Flavor", "Google" } }
                };

                HttpResponseMessage serviceAccountResponse = await httpClient.SendAsync(serviceAccountRequest).ConfigureAwait(false);
                serviceAccountResponse.EnsureSuccessStatusCode();
                return await serviceAccountResponse.Content.ReadAsStringAsync();
            }
        }

        public static async Task<UrlSigner> GetUrlSigner(string serviceAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                // Create an IAM service client object using the default application credentials.
                GoogleCredential iamCredential = await GoogleCredential.GetApplicationDefaultAsync();
                iamCredential = iamCredential.CreateScoped(IamService.Scope.CloudPlatform);
                IamService iamService = new IamService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = iamCredential
                });

                // Create a URL signer that will use the IAM service for signing. This signer is thread-safe,
                // and would typically occur as a dependency, e.g. in an ASP.NET Core controller, where the
                // same instance can be reused for each request.
                IamServiceBlobSigner blobSigner = new IamServiceBlobSigner(iamService, serviceAccountId);
                return UrlSigner.FromBlobSigner(blobSigner);
            }
        }
    }
}