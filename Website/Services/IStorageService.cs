using System.Threading.Tasks;
using Website.Models;

namespace Website.Services
{
    public interface IStorageService
    {
        Task<S3ResponseDto> UploadFileAsync(S3Object obj, AwsCredentials awsCredentialsValues);
    }
}
