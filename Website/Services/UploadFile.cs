using Amazon.Runtime;
using Amazon.S3.Transfer;
using Amazon.S3;
using Website.Models;

namespace Website.Services
{
    public class UploadFile : IStorageService
    {
        private readonly UploadFileContext _context;
        private IConfiguration _configuration;
        public UploadFile(UploadFileContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<List<mFile>> GetAll()
        {
            var result = (
                    from f in _context.Files
                    select new mFile
                    {
                        Name = f.Name,
                        Filepath = f.Filepath,
                    }
                );
            return result.ToList();
        }
        public async Task<mFile> Store(mFile data)
        {
            await using var memoryStream = new MemoryStream();
            await data.Files.CopyToAsync( memoryStream );
            //var filepath = data.Files.ToString();
            var fileExt = Path.GetExtension( data.Files.FileName );
            var fileName = Path.GetFileName(data.Files.FileName);

            var s3Obj = new S3Object()
            {
                BucketName = "graha-tms",
                InputStream = memoryStream,
                Name = fileName,
            };
            var cred = new AwsCredentials()
            {
                AccessKey = _configuration["AwsConfiguration:AWSAccessKey"],
                SecretKey = _configuration["AwsConfiguration:AWSSecretKey"]
            };
            var upload = await UploadFileAsync(s3Obj, cred);

            var input = new mFile()
            {
                Id = data.Id,
                Name = data.Name,
                Filepath = $"https://{s3Obj.BucketName}.s3.ap-southeast-3.amazonaws.com/{s3Obj.Name}"
            };
            _context.Files.Add(input);
            _context.SaveChanges();
            return data;
        }
        public async Task<S3ResponseDto> UploadFileAsync(S3Object obj, AwsCredentials awsCredentialsValues)
        {
            //var awsCredentialsValues = _config.ReadS3Credentials();

            Console.WriteLine($"Key: {awsCredentialsValues.AccessKey}, Secret: {awsCredentialsValues.SecretKey}");

            var credentials = new BasicAWSCredentials(awsCredentialsValues.AccessKey, awsCredentialsValues.SecretKey);

            var config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast3
            };

            var response = new S3ResponseDto();
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest()
                {
                    InputStream = obj.InputStream,
                    Key = obj.Name,
                    BucketName = obj.BucketName,
                    CannedACL = S3CannedACL.NoACL
                };

                // initialise client
                using var client = new AmazonS3Client(credentials, config);

                // initialise the transfer/upload tools
                var transferUtility = new TransferUtility(client);

                // initiate the file upload
                await transferUtility.UploadAsync(uploadRequest);

                response.StatusCode = 201;
                response.Message = $"{obj.Name} has been uploaded sucessfully";
            }
            catch (AmazonS3Exception s3Ex)
            {
                response.StatusCode = (int)s3Ex.StatusCode;
                response.Message = s3Ex.Message;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
