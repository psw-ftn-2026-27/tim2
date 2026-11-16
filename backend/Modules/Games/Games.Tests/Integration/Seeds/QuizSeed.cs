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
        ]);

    public static readonly Quiz DraftCityHall = new(
        "Gradska kuća",
        "Stanite ispred Gradske kuće i pogledajte fasadu.",
        30,
        [
            new Question("Na kom trgu je Gradska kuća?", [new AnswerOption("Trg slobode", true), new AnswerOption("Trg mladenaca", false)])
        ]);

    public static readonly Quiz DraftMuseum = new(
        "Muzej Vojvodine",
        "Prošetajte stalnom postavkom muzeja.",
        50,
        [
            new Question("Koji predmet je najstariji u postavci?", [new AnswerOption("Kameni idol", false), new AnswerOption("Zlatni šlem", false), new AnswerOption("Keramička posuda", true)]),
            new Question("U kojoj ulici je muzej?", [new AnswerOption("Dunavska", true), new AnswerOption("Zmaj Jovina", false)]),
            new Question("Iz kog veka je zgrada muzeja?", [new AnswerOption("Dvadesetog", false), new AnswerOption("Devetnaestog", true)])
        ]);

    public static Quiz[] All => [DraftFortressArchitecture, DraftCityHall, DraftMuseum];
}
