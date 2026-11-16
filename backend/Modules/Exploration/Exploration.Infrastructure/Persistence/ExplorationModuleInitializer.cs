using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            dbContext.Tours.AddRange(CreateDemoTours());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Tour> CreateDemoTours()
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"]);
        fortress.ChangeRecommendedGroupSize(2, 15);
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeyPoint("Sahat kula", "Sat čije su kazaljke zamenjene, pa velika pokazuje sate, a mala minute.",
            "/images/sahat-kula.jpg", new Location(45.2526, 19.8636), null, true);
        fortress.AddKeyPoint("Gornji plato", "Vidikovac sa pogledom na Dunav i stari deo Novog Sada.",
            "/images/gornji-plato.jpg", new Location(45.2522, 19.8618), null, true);
        fortress.AddKeyPoint("Ulaz u podzemne galerije", "Ulaz u vojne galerije ispod tvrđave.",
            "/images/podzemne-galerije.jpg", new Location(45.2508, 19.8649), "Galerije se prostiru na četiri nivoa.", false);
        fortress.SetCalculatedLength(0.9);
        fortress.Publish();

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"]);
        fruskaGora.ChangeRecommendedGroupSize(1, 6);
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeyPoint("Sremska Kamenica", "Polazna tačka kod crkve u centru Sremske Kamenice.",
            "/images/sremska-kamenica.jpg", new Location(45.2228, 19.8392), null, true);
        fruskaGora.AddKeyPoint("Popovica", "Izletište na proplanku, mesto za prvi predah posle uspona.",
            "/images/popovica.jpg", new Location(45.1867, 19.8478), null, true);
        fruskaGora.AddKeyPoint("Iriški venac", "Prevoj sa spomenikom slobodi i pogledom na Srem.",
            "/images/iriski-venac.jpg", new Location(45.1497, 19.8603), "Na vrhu spomenika je vidikovac.", false);
        fruskaGora.SetCalculatedLength(10.4);
        fruskaGora.Publish();

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"]);

        return [fortress, fruskaGora, cityCenter];
    }
}
