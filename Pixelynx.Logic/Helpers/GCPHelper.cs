using System;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Iam.v1;
using Google.Apis.Iam.v1.Data;
using Google.Apis.Services;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static async Task<string> SignJwt(string project, string serviceAccountEmail)
        {
            var credential = await GetCredential();
            IamService iamService = new IamService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            System.TimeSpan timeDifference = DateTime.UtcNow.AddMinutes(15) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixEpochTime = System.Convert.ToInt64(timeDifference.TotalSeconds);

            JObject obj = new JObject();
            obj["sub"] = serviceAccountEmail; 
            obj["aud"] = "vault/my-iam-role";
            obj["exp"] = unixEpochTime;

            string name = $"projects/{project}/serviceAccounts/{serviceAccountEmail}";
            SignJwtRequest requestBody = new SignJwtRequest()
            {
                Payload = obj.ToString(Formatting.None), 
            };

            ProjectsResource.ServiceAccountsResource.SignJwtRequest request = iamService.Projects.ServiceAccounts.SignJwt(requestBody, name);
            SignJwtResponse response = await request.ExecuteAsync();

            return response.SignedJwt;
        }

        public static async Task<UrlSigner> GetUrlSigner()
        {
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage serviceAccountRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://169.254.169.254"),
                    Headers = { { "Metadata-Flavor", "Google" } }
                };

                HttpResponseMessage serviceAccountResponse = await httpClient.SendAsync(serviceAccountRequest).ConfigureAwait(false);
                serviceAccountResponse.EnsureSuccessStatusCode();
                string serviceAccountId = await serviceAccountResponse.Content.ReadAsStringAsync();

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