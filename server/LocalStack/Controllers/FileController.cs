using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using LocalStack.DAL;
using LocalStack.DAL.Models;
using LocalStack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shortid;

namespace LocalStack.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase {
        private readonly ILogger<FileController> p_logger;
        private readonly IAmazonS3 p_s3Client;
        private readonly AppSettings p_settings;
        private readonly LocalStackContext p_ctx;

        public FileController(ILogger<FileController> logger, IAmazonS3 s3Client, AppSettings settings, LocalStackContext ctx) {
            p_logger = logger;
            p_s3Client = s3Client;
            p_settings = settings;
            p_ctx = ctx;

            p_logger.LogInformation("[FileController] constructor");
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] FileFilter filter) {
            p_logger.LogInformation("[FileController] Items fetching");

            var items = await filter.Filter(p_ctx.Items).Select(x => new ItemVM {
                Id = x.Id,
                Name = x.Name,
                DateCreated = x.DateCreated,
                Extension = x.Extension,
                SizeInBytes = x.SizeInBytes,
                Type = x.Type
            }).ToListAsync();

            var breadcrumb = new List<ItemVM>();
            if (!string.IsNullOrEmpty(filter.ParentId)) {
                var currentFolder = await p_ctx.Items.FirstOrDefaultAsync(x => x.Id.Equals(filter.ParentId));
                while (true) {
                    breadcrumb.Add(new ItemVM { Id = currentFolder.Id, Name = currentFolder.Name, Type = ItemType.Folder });
                    if (currentFolder.ParentId == null)
                        break;
                    currentFolder = await p_ctx.Items.FirstOrDefaultAsync(x => x.Id.Equals(currentFolder.ParentId));
                }
            }
            breadcrumb.Reverse();

            return Ok(new {
                items,
                breadcrumb
            });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(string id) {
            p_logger.LogInformation("[FileController] Download");

            var item = await p_ctx.Items.FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (item.Type != ItemType.File) {
                p_logger.LogError("[FileController] Couldn't download item {0}", id);
                throw new InvalidOperationException("Couldn't download item");
            }

            var request = new GetPreSignedUrlRequest {
                BucketName = p_settings.Aws.S3.Bucket,
                Key = id,
                Expires = DateTime.UtcNow.AddDays(1),
                Protocol = Protocol.HTTP
            };
            // tell browser we going to download the file
            request.ResponseHeaderOverrides.ContentDisposition = $"attachment;filename={item.Name}";

            var preSignedUrl = p_s3Client.GetPreSignedURL(request);
            p_logger.LogInformation("[FileController] File download url generated {0}", preSignedUrl);
            return Ok(new {
                url = preSignedUrl
            });
        }

        [HttpPut]
        public async Task<IActionResult> Upload([FromForm] UploadModel model) {
            p_logger.LogInformation("[FileController] File uploading started {0} {1} {2}",
                model.File.FileName, model.File.ContentType, model.File.Length);

            string id = ShortId.Generate(10);
            var item = new Item {
                Id = id,
                DateCreated = DateTime.UtcNow,
                Name = model.File.FileName,
                Extension = model.File.FileName.Split(".").Last(),
                SizeInBytes = model.File.Length,
                Type = ItemType.File,
                ParentId = model.ParentId
            };
            using (var fs = model.File.OpenReadStream()) {
                var request = new PutObjectRequest {
                    BucketName = p_settings.Aws.S3.Bucket,
                    Key = id,
                    InputStream = fs
                };
                var response = await p_s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK) {
                    p_logger.LogError("[FileController] Couldn't upload file {0} {1} {2}",
                        model.File.FileName, model.File.ContentType, model.File.Length);
                    throw new InvalidOperationException("Couldn't upload file");
                }
            }
            p_ctx.Items.Add(item);
            await p_ctx.SaveChangesAsync();

            p_logger.LogInformation("[FileController] File uploading completed {0} {1} {2}",
                model.File.FileName, model.File.ContentType, model.File.Length);
            return Ok(new ItemVM {
                Id = item.Id,
                Name = item.Name,
                DateCreated = item.DateCreated,
                Extension = item.Extension,
                SizeInBytes = item.SizeInBytes,
                Type = item.Type,
            });
        }

        [HttpPost("create-folder")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderModel model) {
            p_logger.LogInformation("[FileController] Folder creating {0}", model.Name);

            var item = new Item {
                Id = ShortId.Generate(10),
                DateCreated = DateTime.UtcNow,
                Name = model.Name,
                Type = ItemType.Folder,
                ParentId = model.ParentId
            };
            p_ctx.Items.Add(item);
            await p_ctx.SaveChangesAsync();

            p_logger.LogInformation("[FileController] Folder created {0}", model.Name);
            return Ok(new ItemVM {
                Id = item.Id,
                Name = item.Name,
                DateCreated = item.DateCreated,
                Extension = item.Extension,
                SizeInBytes = item.SizeInBytes,
                Type = item.Type
            });
        }
    }
}
