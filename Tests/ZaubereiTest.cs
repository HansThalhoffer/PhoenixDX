using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Zaubersprueche ohne Datenbank: das Format der Befehlsspalten und die Kostentabellen
    /// aus Regelwerk 1.4.2 bis 1.4.4.
    /// </summary>
    public class ZaubereiTest {

        [Fact]
        public void EinWandbefehlUeberstehtSchreibenUndLesen() {
            var befehl = new Zauberbefehl {
                Spruch = Zauberspruch.WandErrichten,
                Ziel = new KleinfeldPosition(603, 45),
                Richtung = Direction.NO,
            };
            Assert.Equal("EW:603/45,NO", befehl.ToString());

            var gelesen = Zauberbefehl.Lies(befehl.ToString());
            Assert.NotNull(gelesen);
            Assert.Equal(Zauberspruch.WandErrichten, gelesen!.Spruch);
            Assert.Equal(603, gelesen.Ziel.gf);
            Assert.Equal(45, gelesen.Ziel.kf);
            Assert.Equal(Direction.NO, gelesen.Richtung);
        }

        [Fact]
        public void MehrereBefehleStehenHintereinanderInDerSpalte() {
            List<Zauberbefehl> befehle = [
                new() { Spruch = Zauberspruch.WandErrichten, Ziel = new KleinfeldPosition(603, 45), Richtung = Direction.NO },
                new() { Spruch = Zauberspruch.WandEinreissen, Ziel = new KleinfeldPosition(1002, 87), Richtung = Direction.W },
                new() { Spruch = Zauberspruch.Zauberduell, Ziel = new KleinfeldPosition(701, 33) },
            ];
            string spalte = Zauberbefehl.Schreibe(befehle);
            Assert.Equal("#EW:603/45,NO#ZW:1002/87,W#ZD:701/33", spalte);

            var gelesen = Zauberbefehl.LiesAlle(spalte);
            Assert.Equal(3, gelesen.Count);
            Assert.Equal(Zauberspruch.WandErrichten, gelesen[0].Spruch);
            Assert.Equal(Zauberspruch.WandEinreissen, gelesen[1].Spruch);
            Assert.Equal(Zauberspruch.Zauberduell, gelesen[2].Spruch);
            Assert.Null(gelesen[2].Richtung);
        }

        /// <summary>
        /// Die Spalte kann Altlasten enthalten; was sich nicht lesen laesst, wird uebergangen
        /// statt den ganzen Inhalt zu verwerfen.
        /// </summary>
        [Fact]
        public void UnlesbaresWirdUebergangen() {
            var gelesen = Zauberbefehl.LiesAlle("#XX:1/2#EW:603/45,NO#ZD:603/45,NO#ZW:7/8");
            Assert.Single(gelesen);
            Assert.Equal(Zauberspruch.WandErrichten, gelesen[0].Spruch);
        }

        [Fact]
        public void EinBannbefehlUeberstehtSchreibenUndLesen() {
            var befehl = new Bannbefehl { Ziel = new KleinfeldPosition(603, 45), Raumpunkte = 2000 };
            Assert.Equal("ZB:603/45,2000", befehl.ToString());

            var gelesen = Bannbefehl.Lies(befehl.ToString());
            Assert.NotNull(gelesen);
            Assert.Equal(603, gelesen!.Ziel.gf);
            Assert.Equal(45, gelesen.Ziel.kf);
            Assert.Equal(2000, gelesen.Raumpunkte);
            Assert.Null(Bannbefehl.Lies(string.Empty));
            Assert.Null(Bannbefehl.Lies("EW:603/45,NO"));
        }

        /// <summary>
        /// Regelwerk 1.4.2: in 0 Gemarken 2 ZKP, in 1 Gemark 3 ZKP, in 2 Gemarken 4 ZKP.
        /// Regelwerk 1.4.4: einreissen kostet 1, 2 und 4 ZKP.
        /// </summary>
        [Fact]
        public void DieWandkostenStehenImRegelwerk() {
            Assert.Equal(2, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandErrichten, 0));
            Assert.Equal(3, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandErrichten, 1));
            Assert.Equal(4, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandErrichten, 2));

            Assert.Equal(1, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandEinreissen, 0));
            Assert.Equal(2, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandEinreissen, 1));
            Assert.Equal(4, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandEinreissen, 2));

            // ausserhalb der Reichweite gibt es keine Kosten, sondern gar keinen Spruch
            Assert.Equal(0, ZaubereiRules.BerechneWandkosten(Zauberspruch.WandErrichten, 3));
            Assert.Equal(0, ZaubereiRules.BerechneWandkosten(Zauberspruch.Bannen, 1));
        }

        /// <summary>
        /// Die Banntabelle aus Regelwerk 1.4.3, Zeile fuer Zeile. Sie ist linear - 500 RP je ZKP
        /// auf eine Gemark, 250 RP je ZKP auf zwei - und genau das wird hier nachgerechnet.
        /// </summary>
        [Theory]
        [InlineData(1, 1000, 2)]
        [InlineData(1, 2000, 4)]
        [InlineData(1, 5000, 10)]
        [InlineData(1, 10000, 20)]
        [InlineData(1, 20000, 40)]
        [InlineData(1, 50000, 100)]
        [InlineData(2, 500, 2)]
        [InlineData(2, 1000, 4)]
        [InlineData(2, 2500, 10)]
        [InlineData(2, 5000, 20)]
        [InlineData(2, 10000, 40)]
        [InlineData(2, 25000, 100)]
        public void DieBanntabelleStimmt(int entfernung, int raumpunkte, int zauberkraftpunkte) {
            Assert.Equal(zauberkraftpunkte, ZaubereiRules.BerechneBannkosten(entfernung, raumpunkte));
            Assert.Equal(raumpunkte, ZaubereiRules.BerechneBannwirkung(entfernung, zauberkraftpunkte));
        }

        /// <summary>
        /// Angefangene Zauberkraftpunkte zaehlen voll - sonst liesse sich mit 0 ZKP bannen.
        /// </summary>
        [Fact]
        public void AngefangeneZauberkraftpunkteZaehlenVoll() {
            Assert.Equal(1, ZaubereiRules.BerechneBannkosten(1, 1));
            Assert.Equal(1, ZaubereiRules.BerechneBannkosten(1, 500));
            Assert.Equal(2, ZaubereiRules.BerechneBannkosten(1, 501));
            Assert.Equal(0, ZaubereiRules.BerechneBannkosten(1, 0));
        }

        /// <summary>
        /// Regelwerk 1.4.1: je angefangene 5.000 Raumpunkte verkuerzt sich die Reichweite um eine
        /// Gemark und es kostet 2 ZKP pro Feld.
        /// </summary>
        [Fact]
        public void DieTeleportkostenRechnenInFuenftausenderSchritten() {
            Assert.Equal(0, ZaubereiRules.BerechneTeleportschritte(0));
            Assert.Equal(1, ZaubereiRules.BerechneTeleportschritte(1));
            Assert.Equal(1, ZaubereiRules.BerechneTeleportschritte(5000));
            Assert.Equal(2, ZaubereiRules.BerechneTeleportschritte(5001));
            Assert.Equal(3, ZaubereiRules.BerechneTeleportschritte(12000));

            // 12.000 RP sind drei angefangene Schritte, ueber 2 Felder also 3 * 2 * 2 = 12 ZKP
            Assert.Equal(12, ZaubereiRules.BerechneTeleportkosten(12000, 2));
            Assert.Equal(0, ZaubereiRules.BerechneTeleportkosten(12000, 0));
        }

        /// <summary>
        /// Regelwerk 1.4.2: Klassen A bis C errichten eine Wand pro Monat, ab D zwei.
        /// Regelwerk 1.4.4: Klassen A bis D reissen eine Wand pro Monat ein, ab E zwei.
        /// </summary>
        [Fact]
        public void DasWandpensumHaengtAnDerKlasse() {
            Assert.Equal(1, ZaubereiRules.GetMaxWändeErrichten(Zaubererklasse.ZA));
            Assert.Equal(1, ZaubereiRules.GetMaxWändeErrichten(Zaubererklasse.ZC));
            Assert.Equal(2, ZaubereiRules.GetMaxWändeErrichten(Zaubererklasse.ZD));
            Assert.Equal(2, ZaubereiRules.GetMaxWändeErrichten(Zaubererklasse.ZF));

            Assert.Equal(1, ZaubereiRules.GetMaxWändeEinreissen(Zaubererklasse.ZA));
            Assert.Equal(1, ZaubereiRules.GetMaxWändeEinreissen(Zaubererklasse.ZD));
            Assert.Equal(2, ZaubereiRules.GetMaxWändeEinreissen(Zaubererklasse.ZE));
            Assert.Equal(2, ZaubereiRules.GetMaxWändeEinreissen(Zaubererklasse.ZF));
        }

        [Fact]
        public void DieKlassenstufenGehenVonEinsBisSechs() {
            Assert.Equal(1, ZaubereiRules.GetKlassenstufe(Zaubererklasse.ZA));
            Assert.Equal(6, ZaubereiRules.GetKlassenstufe(Zaubererklasse.ZF));
            Assert.Equal(0, ZaubereiRules.GetKlassenstufe(Zaubererklasse.none));
        }

        [Theory]
        [InlineData("Zauberer 501 errichtet eine magische Wand im Nordosten von 701/33", Zauberspruch.WandErrichten)]
        [InlineData("Zauberer 501 reisst die magische Wand im Westen von 701/33 ein", Zauberspruch.WandEinreissen)]
        [InlineData("Zauberer 501 reißt die magische Wand im W von 701/33 ein", Zauberspruch.WandEinreissen)]
        [InlineData("Zauberer 501 bannt 2000 Raumpunkte auf 701/33", Zauberspruch.Bannen)]
        [InlineData("Zauberer 501 fordert 701/33 zum Zauberduell", Zauberspruch.Zauberduell)]
        [InlineData("Zauberer 501 teleportiert nach 755/22 mit Krieger 101, Reiter 203", Zauberspruch.TeleportMitRüstgütern)]
        public void DieBefehleWerdenErkannt(string eingabe, Zauberspruch erwartet) {
            Assert.True(CommandParser.ParseCommand(eingabe, out var command), $"'{eingabe}' wurde nicht erkannt");
            var zauber = Assert.IsType<CastSpellCommando>(command);
            Assert.Equal(erwartet, zauber.Spell);
            Assert.Equal(501, zauber.UnitID);
        }

        [Fact]
        public void DerBefehlTraegtFeldRichtungUndMenge() {
            Assert.True(CommandParser.ParseCommand("Zauberer 530 errichtet eine magische Wand im Nordosten von 701/33", out var wand));
            var errichten = Assert.IsType<CastSpellCommando>(wand);
            Assert.Equal(Direction.NO, errichten.Richtung);
            Assert.Equal(701, errichten.LocationTo!.gf);
            Assert.Equal(33, errichten.LocationTo.kf);

            Assert.True(CommandParser.ParseCommand("Zauberer 530 bannt 2500 Raumpunkte auf 701/33", out var bann));
            var bannen = Assert.IsType<CastSpellCommando>(bann);
            Assert.Equal(2500, bannen.Raumpunkte);
            Assert.Null(bannen.Richtung);

            Assert.True(CommandParser.ParseCommand("Zauberer 530 teleportiert nach 755/22 mit Krieger 101, Reiter 203", out var tele));
            var teleport = Assert.IsType<CastSpellCommando>(tele);
            Assert.Equal([101, 203], teleport.LadungIds);
        }

        /// <summary>
        /// Ohne Ladung ist die Teleportation kein Zauberspruch, sondern eine Bewegungsfertigkeit
        /// (Regelwerk 1.3) - der Zauberbefehl darf sie deshalb nicht an sich ziehen.
        /// </summary>
        /// <summary>
        /// In Befehl_Teleport stehen zwei verschiedene Dinge: die Teleportation allein ist eine
        /// Bewegungsfertigkeit (Regelwerk 1.3), die mit Ruestguetern ein Zauberspruch (1.4.1).
        /// Wer beides gleich schreibt, sperrt dem Zauberer nach einem Teleport den Spruch des
        /// Monats - deshalb steht der Unterschied im Kuerzel.
        /// </summary>
        [Fact]
        public void DerTeleportbefehlUnterscheidetBewegungUndZauberspruch() {
            var bewegung = new Teleportbefehl {
                Von = new KleinfeldPosition(507, 31),
                Nach = new KleinfeldPosition(507, 32),
            };
            Assert.Equal("ZT:507/31-507/32", bewegung.ToString());

            var spruch = new Teleportbefehl {
                MitRüstgütern = true,
                Von = new KleinfeldPosition(507, 31),
                Nach = new KleinfeldPosition(507, 32),
                Ladung = [101, 203],
            };
            Assert.Equal("ZTR:507/31-507/32,101,203", spruch.ToString());

            var gelesen = Teleportbefehl.Lies(bewegung.ToString());
            Assert.NotNull(gelesen);
            Assert.False(gelesen!.MitRüstgütern);
            Assert.Empty(gelesen.Ladung);

            gelesen = Teleportbefehl.Lies(spruch.ToString());
            Assert.NotNull(gelesen);
            Assert.True(gelesen!.MitRüstgütern);
            Assert.Equal([101, 203], gelesen.Ladung);
            Assert.Equal(507, gelesen.Von.gf);
            Assert.Equal(32, gelesen.Nach.kf);

            Assert.Null(Teleportbefehl.Lies(string.Empty));
            Assert.Null(Teleportbefehl.Lies("EW:603/45,NO"));
        }

        [Fact]
        public void EinTeleportOhneLadungIstKeinZauberspruch() {
            Assert.True(CommandParser.ParseCommand("Zauberer 530 teleportiert nach 755/22", out var command));
            var teleport = Assert.IsType<TeleportCommand>(command);
            Assert.Equal(530, teleport.UnitID);
            Assert.Equal(755, teleport.LocationTo!.gf);
            Assert.Equal(22, teleport.LocationTo.kf);
        }
    }
}
