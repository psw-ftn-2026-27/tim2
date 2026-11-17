using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftFortressArchitecture = new(
        "Arhitektura Petrovaradinske tvrđave",
        "Obiđite tvrđavu i odgovorite na pitanja o njenoj gradnji.",
        80,
        [
            new Question("Ko je projektovao tvrđavu?", [new AnswerOption("Sebastijan Vauban", true), new AnswerOption("Eugen Savojski", false)]),
            new Question("Koliko kilometara podzemnih hodnika ima tvrđava?", [new AnswerOption("16", true), new AnswerOption("2", false), new AnswerOption("40", false)])
        ],
        null);

    public static readonly Quiz DraftCityHall = new(
        "Gradska kuća",
        "Stanite ispred Gradske kuće i pogledajte fasadu.",
        30,
        [
            new Question("Na kom trgu je Gradska kuća?", [new AnswerOption("Trg slobode", true), new AnswerOption("Trg mladenaca", false)])
        ],
        new QuizLocation(45.2551, 19.8451, 50));

    public static readonly Quiz DraftMuseum = new(
        "Muzej Vojvodine",
        "Prošetajte stalnom postavkom muzeja.",
        50,
        [
            new Question("Koji predmet je najstariji u postavci?", [new AnswerOption("Kameni idol", false), new AnswerOption("Zlatni šlem", false), new AnswerOption("Keramička posuda", true)]),
            new Question("U kojoj ulici je muzej?", [new AnswerOption("Dunavska", true), new AnswerOption("Zmaj Jovina", false)]),
            new Question("Iz kog veka je zgrada muzeja?", [new AnswerOption("Dvadesetog", false), new AnswerOption("Devetnaestog", true)])
        ],
        null);

    public static readonly Quiz PublishedSynagogue;
    public static readonly Quiz RetiredBishopsPalace;
    public static readonly Quiz ArchivedZmajJovinaStreet;
    public static readonly Quiz PublishedDanubePark;
    public static readonly Quiz RepeatedFishermansIslandWithMoreXp;
    public static readonly Quiz RetiredSynagogueGarden;

    static QuizSeed()
    {
        PublishedSynagogue = new(
            "Sinagoga",
            "Posetite Sinagogu i pogledajte njenu kupolu.",
            40,
            [
                new Question("Koje godine je završena Sinagoga?", [new AnswerOption("1909", true), new AnswerOption("1850", false)])
            ],
            new QuizLocation(45.2559, 19.8413, 40));
        PublishedSynagogue.Publish();

        RetiredBishopsPalace = new(
            "Vladičanski dvor",
            "Stanite ispred Vladičanskog dvora i pogledajte fasadu.",
            35,
            [
                new Question("Kojim materijalom je ukrašena fasada?", [new AnswerOption("Opekom", true), new AnswerOption("Mermerom", false)])
            ],
            new QuizLocation(45.2556, 19.8449, 30));
        RetiredBishopsPalace.Publish();
        RetiredBishopsPalace.Retire();

        ArchivedZmajJovinaStreet = new(
            "Zmaj Jovina ulica",
            "Prošetajte Zmaj Jovinom ulicom od trga do Dunavske.",
            25,
            [
                new Question("Čiji spomenik stoji na početku ulice?", [new AnswerOption("Jovana Jovanovića Zmaja", true), new AnswerOption("Svetozara Miletića", false)])
            ],
            new QuizLocation(45.2554, 19.8466, 100));
        ArchivedZmajJovinaStreet.Publish();
        ArchivedZmajJovinaStreet.Retire();
        ArchivedZmajJovinaStreet.Archive();

        PublishedDanubePark = new(
            "Park uz Dunav",
            "Prošetajte Dunavskim parkom i odgovorite na pitanja.",
            90,
            [
                new Question("Šta se nalazi na ostrvu u jezeru?", [new AnswerOption("Paviljon", false), new AnswerOption("Vrba", true)]),
                new Question("Koje godine je park uređen?", [new AnswerOption("1895", true), new AnswerOption("1950", false), new AnswerOption("1820", false)]),
                new Question("Čija bista stoji u parku?", [new AnswerOption("Đure Jakšića", true), new AnswerOption("Laze Kostića", false)])
            ],
            new QuizLocation(45.2546, 19.8510, 80));
        PublishedDanubePark.Publish();

        RepeatedFishermansIslandWithMoreXp = new(
            "Ribarsko ostrvo",
            "Obiđite Ribarsko ostrvo i odgovorite na pitanja.",
            60,
            [
                new Question("Na kojoj reci je ostrvo?", [new AnswerOption("Dunav", true), new AnswerOption("Tisa", false)]),
                new Question("Čime je ostrvo poznato?", [new AnswerOption("Ribljom čorbom", true), new AnswerOption("Vinogradima", false)])
            ],
            new QuizLocation(45.2440, 19.8770, 200));
        RepeatedFishermansIslandWithMoreXp.Publish();
        RepeatedFishermansIslandWithMoreXp.Retire();
        RepeatedFishermansIslandWithMoreXp.Repeat();
        RepeatedFishermansIslandWithMoreXp.Update(
            RepeatedFishermansIslandWithMoreXp.Name,
            RepeatedFishermansIslandWithMoreXp.Instructions,
            90,
            [.. RepeatedFishermansIslandWithMoreXp.Questions],
            RepeatedFishermansIslandWithMoreXp.Location);

        // Shares the location and range of PublishedSynagogue, so a player standing there is in range of a quiz that is not published.
        RetiredSynagogueGarden = new(
            "Vrt Sinagoge",
            "Prošetajte vrtom iza Sinagoge.",
            20,
            [
                new Question("Šta raste uz ogradu vrta?", [new AnswerOption("Lipe", true), new AnswerOption("Borovi", false)])
            ],
            new QuizLocation(45.2559, 19.8413, 40));
        RetiredSynagogueGarden.Publish();
        RetiredSynagogueGarden.Retire();
    }

    public static Quiz[] All => [DraftFortressArchitecture, DraftCityHall, DraftMuseum, PublishedSynagogue, RetiredBishopsPalace, ArchivedZmajJovinaStreet, PublishedDanubePark, RepeatedFishermansIslandWithMoreXp, RetiredSynagogueGarden];
}
