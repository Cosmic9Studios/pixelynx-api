using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Iam.v1;
using Google.Apis.Iam.v1.Data;
using Google.Cloud.Storage.V1;

namespace Pixelynx.Logic.Helpers
{
    internal sealed class IamServiceBlobSigner : UrlSigner.IBlobSigner
    {
        private readonly IamService _iamService;
        public string Id { get; }

        internal IamServiceBlobSigner(IamService service, string id)
        {
            _iamService = service;
            Id = id;
        }

        public string CreateSignature(byte[] data) =>
            CreateRequest(data).Execute().Signature;

        public async Task<string> CreateSignatureAsync(byte[] data, CancellationToken cancellationToken)
        {
            ProjectsResource.ServiceAccountsResource.SignBlobRequest request = CreateRequest(data);
            SignBlobResponse response = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            return response.Signature;
        }

        private ProjectsResource.ServiceAccountsResource.SignBlobRequest CreateRequest(byte[] data)
        {
            SignBlobRequest body = new SignBlobRequest { BytesToSign = Convert.ToBase64String(data) };
            string account = $"projects/-/serviceAccounts/{Id}";
            ProjectsResource.ServiceAccountsResource.SignBlobRequest request =
                _iamService.Projects.ServiceAccounts.SignBlob(body, account);
            return request;
        }
    }
}