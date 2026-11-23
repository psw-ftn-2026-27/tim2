using Microsoft.Extensions.DependencyInjection;

namespace Images.Api;

public static class ImagesApiExtensions
{
    public static IMvcBuilder AddImagesControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(ImagesApiExtensions).Assembly);
    }
}
