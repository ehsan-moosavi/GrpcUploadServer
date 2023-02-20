using Grpc.Core;
using GrpcUploadServer;

namespace GrpcUploadServer.Services
{
    public class GrpcUploadServer : Greeter.GreeterBase
    {
        private readonly ILogger<GrpcUploadServer> _logger;
        private readonly IConfiguration _config;
        public GrpcUploadServer(ILogger<GrpcUploadServer> logger,IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
       

        public override Task<UploadFileResponse> UpladFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            var uploadId = Path.GetRandomFileName();
            var uploadPath = Path.Combine(_config["StoredFilesPath"]!, uploadId);
            Directory.CreateDirectory(uploadPath);

            await using var writeStream = File.Create(Path.Combine(uploadPath, "data.bin"));
            await foreach (var message in requestStream.ReadAllAsync())
            {
                if (message.Metadata != null)
                {
                    await File.WriteAllTextAsync(Path.Combine(uploadPath, "metadata.json"), message.Metadata.ToString());
                }
                if (message.Data != null)
                {
                    await writeStream.WriteAsync(message.Data.Memory);
                }
            }
            return new UploadFileResponse { Id = uploadId };
        }
    }
}