using PhoenixModel.Commands;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
namespace Tests {


    public class SimpleParserTests {
        [Theory]
        [InlineData("503/21", 503, 21)]
        [InlineData("503/16", 503, 16)]
        [InlineData("503/ 4", 503, 4)] // Handles space
        [InlineData("603/27", 603, 27)]
        public void ParseLocation_ShouldReturnCorrectKleinfeldPosition(string input, int expectedGf, int expectedKf) {
            // Act
            KleinfeldPosition? result = SimpleParser.ParseLocation(input);

            // Assert
            Assert.NotNull(result); // Ensure it doesn't return null
            Assert.Equal(expectedGf, result.gf);
            Assert.Equal(expectedKf, result.kf);
        }

        [Theory]
        [InlineData("NO", Direction.NO)]
        [InlineData("Nordost", Direction.NO)]
        [InlineData("Nordosten", Direction.NO)]
        [InlineData("O", Direction.O)]
        [InlineData("Ost", Direction.O)]
        [InlineData("Osten", Direction.O)]
        [InlineData("SO", Direction.SO)]
        [InlineData("Südost", Direction.SO)]
        [InlineData("Südosten", Direction.SO)]
        [InlineData("SW", Direction.SW)]
        [InlineData("Südwest", Direction.SW)]
        [InlineData("Südwesten", Direction.SW)]
        [InlineData("W", Direction.W)]
        [InlineData("West", Direction.W)]
        [InlineData("Westen", Direction.W)]
        [InlineData("NW", Direction.NW)]
        [InlineData("Nordwest", Direction.NW)]
        [InlineData("Nordwesten", Direction.NW)]
        [InlineData("Unbekannt", null)] // Test für nicht erkannte Eingaben
        public void ParseDirection_ReturnsCorrectDirection(string input, Direction? expected) {
            // Act
            var result = SimpleParser.ParseDirection(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Reiter", FigurType.Reiter)]
        [InlineData("Krieger", FigurType.Krieger)]
        [InlineData("Schiff", FigurType.Schiff)]
        [InlineData("Zauberer", FigurType.Zauberer)]
        [InlineData("Charakter", FigurType.Charakter)]
        [InlineData("Charakterzauberer", FigurType.CharakterZauberer)]
        [InlineData("LKP", FigurType.LeichteArtillerie)]
        [InlineData("SKP", FigurType.SchwereArtillerie)]
        [InlineData("LKS", FigurType.LeichtesKriegsschiff)]
        [InlineData("SKS", FigurType.SchweresKriegsschiff)]
        [InlineData("Leichtes Katapult", FigurType.LeichteArtillerie)]
        [InlineData("Leichtem Katapult", FigurType.LeichteArtillerie)]
        [InlineData("Leichten Katapulten", FigurType.LeichteArtillerie)]
        [InlineData("Schweres Katapult", FigurType.SchwereArtillerie)]
        [InlineData("Schwerem Katapult", FigurType.SchwereArtillerie)]
        [InlineData("Schweren Katapulten", FigurType.SchwereArtillerie)]
        [InlineData("Leichtes Kriegsschiff", FigurType.LeichtesKriegsschiff)]
        [InlineData("Leichtem Kriegsschiff", FigurType.LeichtesKriegsschiff)]
        [InlineData("Leichten Kriegsschiffen", FigurType.LeichtesKriegsschiff)]
        [InlineData("Schweres Kriegsschiff", FigurType.SchweresKriegsschiff)]
        [InlineData("Schwerem Kriegsschiff", FigurType.SchweresKriegsschiff)]
        [InlineData("Schweren Kriegsschiffen", FigurType.SchweresKriegsschiff)]
        [InlineData("Piratenschiff", FigurType.PiratenSchiff)]
        [InlineData("Piratenschiffen", FigurType.PiratenSchiff)]
        [InlineData("Schweres Piratenkriegsschiff", FigurType.PiratenSchweresKriegsschiff)]
        [InlineData("Schwerem Piratenkriegsschiff", FigurType.PiratenSchweresKriegsschiff)]
        [InlineData("Schweren Piratenkriegsschiffen", FigurType.PiratenSchweresKriegsschiff)]
        [InlineData("Leichtes Piratenkriegsschiff", FigurType.PiratenLeichtesKriegsschiff)]
        [InlineData("Leichtem Piratenkriegsschiff", FigurType.PiratenLeichtesKriegsschiff)]
        [InlineData("Leichten Piratenkriegsschiffen", FigurType.PiratenLeichtesKriegsschiff)]
        [InlineData("Berittene Schwere Artillerie", FigurType.BeritteneSchwereArtillerie)]
        [InlineData("Berittener Schwerer Artillerie", FigurType.BeritteneSchwereArtillerie)]
        [InlineData("Berittene Leichte Artillerie", FigurType.BeritteneLeichteArtillerie)]
        [InlineData("Berittener Leichter Artillerie", FigurType.BeritteneLeichteArtillerie)]
        [InlineData("Unbekannt", FigurType.None)] // Test für nicht erkannte Eingaben
        public void ParseUnitType_ReturnsCorrectFigurType(string input, FigurType expected) {
            // Act
            var result = SimpleParser.ParseUnitType(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
