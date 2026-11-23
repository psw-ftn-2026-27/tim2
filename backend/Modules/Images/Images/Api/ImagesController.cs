using Images.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Images.Api;

[ApiController]
[Authorize]
[Route("api/images")]
public sealed class ImagesController : ControllerBase
{
    // A little above the 15MB domain limit, so a slightly larger file still reaches ImageFile and gets its reason.
    private const long RequestSizeLimitInBytes = 16 * 1024 * 1024;

    private readonly ImageUploading _imageUploading;

    public ImagesController(ImageUploading imageUploading)
    {
        _imageUploading = imageUploading;
    }

    [HttpPost]
    [RequestSizeLimit(RequestSizeLimitInBytes)]
    public async Task<ActionResult<UploadedImageDto>> Upload(IFormFile file)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);

        return await _imageUploading.UploadAsync(content.ToArray());
    }
}
