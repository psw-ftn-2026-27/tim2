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
    }

    public static Quiz[] All => [DraftFortressArchitecture, DraftCityHall, DraftMuseum, PublishedSynagogue, RetiredBishopsPalace, ArchivedZmajJovinaStreet];
}
