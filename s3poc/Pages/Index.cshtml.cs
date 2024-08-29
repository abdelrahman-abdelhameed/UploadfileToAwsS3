using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace s3poc.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        
        [BindProperty] public IFormFile File { get; set; }
        
        const string bucketName = "";
        private const string s3Key = "";
        private const string s3Secret = "";
        
        
        public async Task<IActionResult> OnGet()
        {
            
             
                try
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = bucketName, // Bucket Name
                        Key = "" // your file key
                    };

                    var _s3Client = new AmazonS3Client(s3Key, s3Secret, RegionEndpoint.USEast1);
                    using (var response = await _s3Client.GetObjectAsync(request))
                    using (var memoryStream = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(memoryStream);
                        Console.WriteLine("File downloaded to memory.");
                        byte [] data = memoryStream.ToArray(); // Return the file as a byte array
                        
                        return File(data, "application/pdf", "downloaded.pdf");
                    }
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unknown error encountered on server. Message:'{e.Message}' when writing an object");
                    throw;
                }
            

        }

      

        public async Task OnPostAsync()
        {
            var file = File;


            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    var putRequest = new PutObjectRequest
                    {
                        BucketName =bucketName,
                        Key = $"{Guid.NewGuid()}_{file.FileName}",
                        InputStream = memoryStream, // Use stream instead of file path
                        ContentType = "application/pdf",
                        CannedACL = S3CannedACL.Private
                    };

                    var _s3Client = new AmazonS3Client(s3Key, s3Secret, RegionEndpoint.USEast1);

                    var response = await _s3Client.PutObjectAsync(putRequest);

                    Console.WriteLine($"File uploaded to S3 with ETag: {response.ETag}");
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unknown error encountered on server. Message:'{e.Message}' when writing an object");
            }
        }
    }
}