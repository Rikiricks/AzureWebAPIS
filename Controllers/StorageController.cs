using AspNetRestAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetRestAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StorageController : Controller
    {
        private readonly AzureBlobService _azureBlobService;
        private readonly IConfiguration _configuration;

        public StorageController(IConfiguration configuration)
        {
            _configuration = configuration;
            _azureBlobService = new AzureBlobService(_configuration["Storage:ConnectionStr"], _configuration["Storage:ContainerName"]);
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadToBlobStorage(List<IFormFile> formFiles)
        {
            var response = await _azureBlobService.UploadFiles(formFiles);
            return Ok(response);
        }
    }
}
