using Xunit;

namespace Host.Tests.DependencyTests;

// Another module is reachable only through its Contracts project, Identity only through Identity.Contracts,
// and Images not at all: modules keep only the address of an uploaded image.
public class ModuleIsolationTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_reaches_other_modules_only_through_their_contracts(string module)
    {
        foreach (var other in Modules.Where(other => other != module))
        {
            var forbidden = Layers.Where(layer => layer != "Contracts")
                .Select(layer => $"{other}.{layer}")
                .ToArray();
            foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
            {
                AssertNoDependency(assembly, forbidden);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_identity(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Identity");
        }
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_does_not_depend_on_images(string module)
    {
        foreach (var assembly in Layers.Select(layer => $"{module}.{layer}"))
        {
            AssertNoDependency(assembly, "Images");
        }
    }

    [Fact]
    public void Images_does_not_depend_on_any_module()
    {
        var moduleAssemblies = Modules
            .SelectMany(module => Layers.Select(layer => $"{module}.{layer}"))
            .Append("Identity")
            .Append("Identity.Contracts")
            .ToArray();
        AssertNoDependency("Images", moduleAssemblies);
    }

    [Fact]
    public void Identity_contracts_depends_on_nothing()
    {
        AssertNoDependency("Identity.Contracts",
            "Identity", "Shared.Domain", "Shared.Api", "Shared.Infrastructure");
    }
}
