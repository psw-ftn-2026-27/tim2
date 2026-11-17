using Microsoft.Extensions.DependencyInjection;

namespace Positions.Api;

public static class PositionsApiExtensions
{
    public static IMvcBuilder AddPositionsControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(PositionsApiExtensions).Assembly);
    }
}
