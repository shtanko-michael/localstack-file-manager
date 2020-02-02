using Microsoft.AspNetCore.Http;

namespace LocalStack.Controllers {
    public class UploadModel {
        public IFormFile File { get; set; }
        public string ParentId { get; set; }
    }
}
