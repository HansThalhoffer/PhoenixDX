using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für die Zauberei (Regelwerk Kapitel 1.3 und 1.4).
    ///
    /// Die Anwendung zaubert nicht - sie nimmt den Spruch auf, prüft ihn, zieht die
    /// Zauberkraftpunkte ab und legt den Befehl in den Spalten Befehl_magie, Befehl_bannt und
    /// Befehl_Teleport ab. Gewürfelt und ausgewertet wird bei der Spielleitung.
    ///
    /// Die Altanwendung hat die Zauberei nie zu Ende gebaut: alle vier Dialoge werfen eine
    /// NotImplementedException, ihre Rümpfe sind auskommentiert. Die Kosten hier sind aus diesen
    /// Rümpfen übernommen und decken sich mit dem Regelwerk; die Prüfungen stammen aus dem
    /// Regelwerk.
    ///
    /// Begriffe: der Gutpunktwert (GP_ges) ist die Obergrenze der Zauberkraft, der
    /// Zauberkraftwert (GP_akt) die aktuell verfügbare Zauberkraft in Punkten - die ZKP, mit denen
    /// gezahlt wird. Ein Zauberer wechselt die Klasse, sobald sein Gutpunktwert eine Klassengrenze
    /// über- oder unterschreitet; die Grenzen stehen in crossref_zauberer_teleport.
    /// </summary>
    public static class ZaubereiRules {

        /// <summary>
        /// Eine magische Wand lässt sich "in 0, 1 oder 2 Gemarken Entfernung zum Zauberer
        /// errichten" (Regelwerk 1.4.2), einreissen ebenso (1.4.4).
        /// </summary>
        public const int MaxEntfernungWand = 2;

        /// <summary>
        /// Gebannt wird "in ein oder zwei Gemarken Entfernung" (Regelwerk 1.4.3) - auf das eigene
        /// Feld also nicht.
        /// </summary>
        public const int MinEntfernungBann = 1;
        public const int MaxEntfernungBann = 2;

        /// <summary>
        /// Ein Zauberduell kommt zustande, wenn Zauberer verfeindeter Reiche "in der gleichen oder
        /// in benachbarten Gemarken stehen" (Regelwerk 5.3).
        /// </summary>
        public const int MaxEntfernungDuell = 1;

        /// <summary>
        /// Die Entfernung, ab der eine Wand nur noch ab Klasse ZB gezaubert werden kann
        /// (Regelwerk 1.4.2 und 1.4.4)
        /// </summary>
        public const int EntfernungNurAbZB = 2;

        /// <summary>
        /// Kosten für das Errichten einer magischen Wand, nach Entfernung in Gemarken:
        /// 0 Gemarken 2 ZKP, 1 Gemark 3 ZKP, 2 Gemarken 4 ZKP (Regelwerk 1.4.2).
        /// </summary>
        public static readonly int[] KostenWandErrichten = [2, 3, 4];

        /// <summary>
        /// Kosten für das Einreissen einer magischen Wand, nach Entfernung in Gemarken:
        /// 0 Gemarken 1 ZKP, 1 Gemark 2 ZKP, 2 Gemarken 4 ZKP (Regelwerk 1.4.4).
        /// </summary>
        public static readonly int[] KostenWandEinreissen = [1, 2, 4];

        /// <summary>
        /// Wieviele Raumpunkte ein Zauberkraftpunkt auf eine Gemark Entfernung bannt.
        ///
        /// Die Tabelle in Regelwerk 1.4.3 ist linear: 1.000 RP kosten 2 ZKP, 2.000 RP 4 ZKP,
        /// 5.000 RP 10 ZKP und so fort bis 50.000 RP für 100 ZKP - also 500 RP je ZKP.
        /// </summary>
        public const int RaumpunkteProZKPNah = 500;

        /// <summary>
        /// Dasselbe auf zwei Gemarken Entfernung: dort steht in der Tabelle für denselben
        /// Kraftaufwand jeweils die halbe Menge, also 250 RP je ZKP.
        /// </summary>
        public const int RaumpunkteProZKPFern = 250;

        /// <summary>
        /// Die Teleportation mit Rüstgütern rechnet in Schritten von 5.000 Raumpunkten
        /// (Regelwerk 1.4.1)
        /// </summary>
        public const int RaumpunkteProTeleportschritt = 5000;

        /// <summary>
        /// Je angefangene 5.000 Raumpunkte kostet die Teleportation "2 Punkte Zauberkraft pro
        /// Feld" (Regelwerk 1.4.1)
        /// </summary>
        public const int ZKPProTeleportschrittUndFeld = 2;

        /// <summary>
        /// Die Teleportation mit Rüstgütern beherrschen "erst Zauberer der Klasse E und aufwärts"
        /// (Regelwerk 1.4.1)
        /// </summary>
        public const Zaubererklasse MindestklasseTeleportMitRüstgütern = Zaubererklasse.ZE;

        /// <summary>
        /// "Jeder Zauberer stirbt im Zauberduell, sobald sein Gutpunktwert unter die Grenze von 1
        /// sinkt" (Errata 24 zu Regelwerk 5.3)
        /// </summary>
        public const int MinGutpunkte = 1;

        /// <summary>
        /// Die Klassenstufe, die in die Gutpunktformel des Zauberduells eingeht (Regelwerk 5.3):
        /// ZA zählt 1, ZF zählt 6.
        /// </summary>
        public static int GetKlassenstufe(Zaubererklasse klasse) => klasse switch {
            Zaubererklasse.ZA => 1,
            Zaubererklasse.ZB => 2,
            Zaubererklasse.ZC => 3,
            Zaubererklasse.ZD => 4,
            Zaubererklasse.ZE => 5,
            Zaubererklasse.ZF => 6,
            _ => 0,
        };

        /// <summary>
        /// Die Klassenstufe zu einer Gutpunktzahl, wie sie die Formel des Zauberduells für die
        /// Summe einer Seite braucht. Die Grenzen stehen in crossref_zauberer_teleport; liegt die
        /// Summe über der höchsten Grenze, gilt die höchste Klasse.
        /// </summary>
        public static int GetKlassenstufe(int gutpunkte) {
            var referenz = SharedData.Crossref_zauberer_teleport;
            if (referenz == null)
                return 0;
            int stufe = 0;
            foreach (var eintrag in referenz.OrderBy(e => e.GP)) {
                stufe++;
                if (gutpunkte <= eintrag.GP)
                    return stufe;
            }
            return stufe;
        }

        /// <summary>
        /// Wieviele magische Wände ein Zauberer im Monat errichten kann: "Zauberer der Klassen A
        /// bis C können eine magische Wand pro Monat errichten, ab Klasse D kann ein Magier zwei
        /// magische Wände zur gleichen Zeit errichten." (Regelwerk 1.4.2)
        /// </summary>
        public static int GetMaxWändeErrichten(Zaubererklasse klasse)
            => GetKlassenstufe(klasse) >= GetKlassenstufe(Zaubererklasse.ZD) ? 2 : 1;

        /// <summary>
        /// Wieviele magische Wände ein Zauberer im Monat einreissen kann: "Zauberer der Klassen A
        /// bis D können eine magische Wand pro Monat einreissen, ab Klasse E kann ein Magier zwei
        /// magische Wände zur gleichen Zeit einreissen." (Regelwerk 1.4.4)
        /// </summary>
        public static int GetMaxWändeEinreissen(Zaubererklasse klasse)
            => GetKlassenstufe(klasse) >= GetKlassenstufe(Zaubererklasse.ZE) ? 2 : 1;

        /// <summary>
        /// Der Standort eines Zauberers: hat er sich bewegt, zählt das Zielfeld.
        /// </summary>
        public static KleinfeldPosition GetStandort(Zauberer zauberer)
            => new(zauberer.gf_nach > 0 ? zauberer.gf_nach : zauberer.gf_von,
                   zauberer.gf_nach > 0 ? zauberer.kf_nach : zauberer.kf_von);

        /// <summary>
        /// Alle Zauberer des eigenen Reiches, die auf dieser Gemark stehen.
        ///
        /// Fremde Zauberer stehen nicht in den eigenen Zugdaten - was auf gegnerischer Seite auf
        /// einer Gemark steht, weiss nur die Spielleitung.
        /// </summary>
        public static List<Zauberer> GetZaubererAufGemark(KleinfeldPosition? gemark) {
            List<Zauberer> ergebnis = [];
            if (gemark == null || SharedData.Zauberer == null)
                return ergebnis;
            foreach (var zauberer in SharedData.Zauberer) {
                if (Plausibilität.IsValid(zauberer) == false)
                    continue;
                if (GetStandort(zauberer).Equals(gemark))
                    ergebnis.Add(zauberer);
            }
            return ergebnis;
        }

        /// <summary>
        /// Die Zauberkraft, mit der ein Zauberer in ein Duell geht.
        ///
        /// "Stehen mehrere eigene oder alliierte Zauberer auf einer Gemark, so neutralisieren sie
        /// sich gegenseitig. Haben die Zauberer alle die gleiche GP Zahl so sind diese
        /// neutralisiert, ansonsten wird die Differenz des mächtigsten Zauberers zu dem
        /// zweitmächtigsten genommen." (Regelwerk 1.3)
        ///
        /// Es bleibt also nur dem stärksten Zauberer etwas übrig, und zwar der Abstand zum
        /// zweitstärksten; alle anderen stehen auf 0. Gerangelt wird nach dem Gutpunktwert,
        /// abgezogen wird von der Zauberkraft - beide Beispielreihen zeigen das so:
        /// "Noname1 4/4 und Noname2 14/14 neutralisieren sich so das nun Noname1 = 0/4 und
        /// Noname2 nun 10/14 hat" (Regelwerk 1.3) und "Z1 6/6 und Z2 3/3 ... neutralisieren sich
        /// auf Z1 6/3 und Z2 3/0" (Errata 31). Die Gutpunktwerte bleiben stehen, nur die
        /// Zauberkraft sinkt.
        ///
        /// Die neutralisierten Punkte zählen im Duell als verbrauchte Zauberkraft, also als Würfel
        /// mit dem festen Ergebnis 1 (Errata 31 zu Regelwerk 5.3).
        /// </summary>
        public static int GetWirksameZauberkraft(Zauberer zauberer) {
            var aufDerGemark = GetZaubererAufGemark(GetStandort(zauberer));
            if (aufDerGemark.Count <= 1)
                return zauberer.GP_akt;

            var sortiert = aufDerGemark.OrderByDescending(z => z.GP_ges).ToList();
            if (ReferenceEquals(sortiert[0], zauberer) == false)
                return 0;
            return Math.Max(0, Math.Min(zauberer.GP_akt, sortiert[0].GP_ges - sortiert[1].GP_ges));
        }

        /// <summary>
        /// Ist der Zauberer durch andere Zauberer auf seiner Gemark neutralisiert?
        /// </summary>
        public static bool IstNeutralisiert(Zauberer zauberer)
            => GetWirksameZauberkraft(zauberer) < zauberer.GP_akt;

        /// <summary>
        /// Die Entfernung vom Zauberer zu einer Gemark, gemessen von Feld zu Feld.
        /// </summary>
        /// <returns>die Entfernung, oder -1 wenn sie grösser als maxEntfernung ist</returns>
        public static int GetEntfernung(Zauberer zauberer, KleinfeldPosition? ziel, int maxEntfernung)
            => FernkampfRules.GetEntfernung(GetStandort(zauberer), ziel, maxEntfernung);

        /// <summary>
        /// Die Zauberkraftpunkte, die ein Bann über die Entfernung für die Menge Raumpunkte kostet.
        /// Angefangene Punkte zählen voll.
        /// </summary>
        public static int BerechneBannkosten(int entfernung, int raumpunkte) {
            if (raumpunkte <= 0)
                return 0;
            int proPunkt = entfernung >= MaxEntfernungBann ? RaumpunkteProZKPFern : RaumpunkteProZKPNah;
            return (raumpunkte + proPunkt - 1) / proPunkt;
        }

        /// <summary>
        /// Wieviele Raumpunkte sich über die Entfernung mit soviel Zauberkraft bannen lassen
        /// </summary>
        public static int BerechneBannwirkung(int entfernung, int zauberkraftpunkte) {
            if (zauberkraftpunkte <= 0)
                return 0;
            int proPunkt = entfernung >= MaxEntfernungBann ? RaumpunkteProZKPFern : RaumpunkteProZKPNah;
            return zauberkraftpunkte * proPunkt;
        }

        /// <summary>
        /// Die Kosten eines Wandzaubers nach Entfernung
        /// </summary>
        /// <returns>die Kosten in ZKP, oder 0 wenn die Entfernung ausserhalb der Reichweite liegt</returns>
        public static int BerechneWandkosten(Zauberspruch spruch, int entfernung) {
            if (entfernung < 0 || entfernung > MaxEntfernungWand)
                return 0;
            return spruch switch {
                Zauberspruch.WandErrichten => KostenWandErrichten[entfernung],
                Zauberspruch.WandEinreissen => KostenWandEinreissen[entfernung],
                _ => 0,
            };
        }

        /// <summary>
        /// Die angefangenen 5.000-Raumpunkt-Schritte einer Teleportladung
        /// </summary>
        public static int BerechneTeleportschritte(int raumpunkte)
            => raumpunkte <= 0 ? 0 : (raumpunkte + RaumpunkteProTeleportschritt - 1) / RaumpunkteProTeleportschritt;

        /// <summary>
        /// Die Reichweite einer Teleportation mit Rüstgütern in Gemarken.
        ///
        /// "Er verkürzt pro 5000 Raumpunkte zu transportierender Rüstgüter die zuvor angegebene
        /// Reichweite um je eine Gemark" (Regelwerk 1.4.1). Die Ausgangsreichweite ist die
        /// Teleportweite der Klasse aus crossref_zauberer_teleport, vermindert um die in diesem
        /// Monat schon verbrauchten Teleportpunkte.
        /// </summary>
        public static int BerechneTeleportreichweite(Zauberer zauberer, int raumpunkte)
            => Math.Max(0, GetFreieTeleportpunkte(zauberer) - BerechneTeleportschritte(raumpunkte));

        /// <summary>
        /// Die in diesem Monat noch nicht verbrauchte Teleportweite.
        ///
        /// Die Spalte tp zählt die verbrauchten Teleportpunkte mit, die Obergrenze steht in
        /// crossref_zauberer_teleport. Die Altanwendung hat tp bei jedem teleportierten Feld um
        /// eins erhöht und gegen dieselbe Obergrenze geprüft.
        /// </summary>
        public static int GetFreieTeleportpunkte(Zauberer zauberer)
            => Math.Max(0, zauberer.MaxTeleportPunkte - zauberer.tp);

        /// <summary>
        /// Die Zauberkraftpunkte, die eine Teleportation mit Rüstgütern kostet:
        /// "2 Punkte Zauberkraft pro Feld" je angefangene 5.000 Raumpunkte (Regelwerk 1.4.1).
        /// </summary>
        public static int BerechneTeleportkosten(int raumpunkte, int entfernung) {
            if (entfernung <= 0)
                return 0;
            return BerechneTeleportschritte(raumpunkte) * ZKPProTeleportschrittUndFeld * entfernung;
        }

        /// <summary>
        /// Die Sprüche, die der Zauberer in diesem Monat schon aufgegeben hat.
        /// Das Duell zählt nicht mit, es kostet keine Zauberkraft und ist kein Zauberspruch.
        /// </summary>
        public static List<Zauberbefehl> GetAufgegebeneSprüche(Zauberer zauberer)
            => Zauberbefehl.LiesAlle(zauberer.Befehl_magie)
                .Where(befehl => befehl.Spruch != Zauberspruch.Zauberduell)
                .ToList();

        /// <summary>
        /// Hat der Zauberer in diesem Monat schon gezaubert?
        ///
        /// "Zauberer können generell nur einen Zauberspruch pro Monat machen, er kann aber z.B. bei
        /// der magischen Wand aus mehreren Wänden bestehen." (Regelwerk 1.4) Ein Zauberer darf also
        /// zwei Wände derselben Art zaubern, aber nicht bannen und eine Wand errichten.
        /// </summary>
        public static Zauberspruch GetLaufendenSpruch(Zauberer zauberer) {
            if (Bannbefehl.Lies(zauberer.Befehl_bannt) != null)
                return Zauberspruch.Bannen;
            // In Befehl_Teleport steht auch die Teleportation ohne Ladung, und die ist eine
            // Bewegungsfertigkeit und kein Zauberspruch (Regelwerk 1.3).
            if (Teleportbefehl.Lies(zauberer.Befehl_Teleport)?.MitRüstgütern == true)
                return Zauberspruch.TeleportMitRüstgütern;
            var sprüche = GetAufgegebeneSprüche(zauberer);
            return sprüche.Count == 0 ? Zauberspruch.Keiner : sprüche[0].Spruch;
        }

        /// <summary>
        /// Die Bezeichnung eines Spruchs für Meldungen
        /// </summary>
        public static string GetBezeichnung(Zauberspruch spruch) => spruch switch {
            Zauberspruch.WandErrichten => "eine magische Wand errichten",
            Zauberspruch.WandEinreissen => "eine magische Wand einreissen",
            Zauberspruch.Zauberduell => "ein Zauberduell fordern",
            Zauberspruch.Bannen => "bannen",
            Zauberspruch.TeleportMitRüstgütern => "mit Rüstgütern teleportieren",
            _ => "nichts",
        };

        /// <summary>
        /// Die Prüfungen, die für jeden Zauberspruch gelten: Phase, Reich, Neutralisierung durch
        /// andere Zauberer auf derselben Gemark und die Grenze von einem Spruch pro Monat.
        /// </summary>
        public static Result PrüfeGrundbedingungen(Zauberer? zauberer, Zauberspruch spruch) {
            if (zauberer == null)
                return Result.Fail("Es ist kein Zauberer ausgewählt", "Ohne Zauberer lässt sich kein Spruch aufgeben.");

            if (zauberer.Nation != ProgramView.SelectedNation)
                return Result.Fail($"{zauberer.Bezeichner} gehört nicht zum eigenen Reich",
                    "Befehle lassen sich nur für eigene Figuren geben.");

            if (ZugView.KannBewegen == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht gezaubert",
                    "Die Zaubersprüche gehören zum Spielzug; erst wenn die Rüstphase abgeschlossen ist, "
                    + "können Sprüche aufgegeben werden.");

            if (zauberer.GP_ges < MinGutpunkte)
                return Result.Fail($"{zauberer.Bezeichner} hat keine Gutpunkte mehr",
                    "Ein Zauberer stirbt, sobald sein Gutpunktwert unter 1 sinkt (Errata 24 zu Regelwerk 5.3).");

            // "Stehen mehrere Zauberer auf einer Gemark so können sie nicht zaubern." (Regelwerk 1.4)
            var mitbewohner = GetZaubererAufGemark(GetStandort(zauberer));
            if (mitbewohner.Count > 1)
                return Result.Fail($"Auf {GetStandort(zauberer).CreateBezeichner()} stehen mehrere Zauberer",
                    $"Dort stehen {string.Join(", ", mitbewohner.Select(z => z.Bezeichner))}. Stehen mehrere "
                    + "Zauberer auf einer Gemark, so können sie nicht zaubern (Regelwerk 1.4); sie "
                    + "neutralisieren sich gegenseitig.");

            var laufend = GetLaufendenSpruch(zauberer);
            if (laufend != Zauberspruch.Keiner && laufend != spruch)
                return Result.Fail($"{zauberer.Bezeichner} hat in diesem Monat schon gezaubert",
                    $"Aufgegeben ist bereits: {GetBezeichnung(laufend)}. Ein Zauberer kann nur einen "
                    + "Zauberspruch pro Monat machen (Regelwerk 1.4).");

            return Result.Success();
        }

        /// <summary>
        /// Prüft, ob der Zauberer genug Zauberkraft für den Spruch hat
        /// </summary>
        private static Result PrüfeZauberkraft(Zauberer zauberer, int kosten) {
            if (kosten > zauberer.GP_akt)
                return Result.Fail($"{zauberer.Bezeichner} hat nicht genug Zauberkraft",
                    $"Der Spruch kostet {kosten} Zauberkraftpunkte, verfügbar sind {zauberer.GP_akt}.");
            return Result.Success();
        }

        /// <summary>
        /// Prüft das Errichten oder Einreissen einer magischen Wand, ohne etwas zu verändern.
        ///
        /// Die Wand liegt zwischen zwei Gemarken; angegeben werden das Feld und die Gemarkseite.
        /// Gemessen wird die Entfernung zu dem Feld, an dem die Wand liegt.
        /// </summary>
        /// <param name="kosten">die ermittelten Kosten in ZKP, wenn die Prüfung durchgeht</param>
        public static Result PrüfeWand(Zauberer? zauberer, Zauberspruch spruch, KleinfeldPosition? feld,
                                       Direction? richtung, out int kosten) {
            kosten = 0;
            if (spruch != Zauberspruch.WandErrichten && spruch != Zauberspruch.WandEinreissen)
                return Result.Fail("Das ist kein Wandzauber", GetBezeichnung(spruch));

            var grund = PrüfeGrundbedingungen(zauberer, spruch);
            if (grund.HasErrors || zauberer == null)
                return grund;

            if (feld == null || richtung == null)
                return Result.Fail("Die Wand ist nicht vollständig angegeben",
                    "Eine magische Wand liegt an einer Gemarkseite; dafür braucht es das Feld und die Richtung.");

            if (KleinfeldView.GetKleinfeld(feld) == null)
                return Result.Fail("Das Feld liegt nicht auf der Karte", feld.CreateBezeichner());
            if (KartenKoordinaten.GetNachbar(feld, richtung.Value) == null)
                return Result.Fail("Dort gibt es keine Gemarkseite",
                    $"Im {richtung} von {feld.CreateBezeichner()} liegt kein Nachbarfeld, zwischen die beiden "
                    + "passt also keine Wand.");

            int entfernung = GetEntfernung(zauberer, feld, MaxEntfernungWand);
            if (entfernung < 0)
                return Result.Fail($"{feld.CreateBezeichner()} liegt ausserhalb der Reichweite",
                    $"Eine magische Wand lässt sich in 0, 1 oder 2 Gemarken Entfernung zum Zauberer zaubern "
                    + $"(Regelwerk 1.4.2). {zauberer.Bezeichner} steht auf {GetStandort(zauberer).CreateBezeichner()}.");

            if (entfernung >= EntfernungNurAbZB && GetKlassenstufe(zauberer.Klasse) < GetKlassenstufe(Zaubererklasse.ZB))
                return Result.Fail($"{zauberer.Bezeichner} reicht nicht so weit",
                    $"Auf {entfernung} Gemarken Entfernung kann erst ein Zauberer ab Klasse ZB wirken, "
                    + $"{zauberer.Bezeichner} ist {zauberer.Klasse}.");

            int schonGezaubert = GetAufgegebeneSprüche(zauberer).Count(befehl => befehl.Spruch == spruch);
            int erlaubt = spruch == Zauberspruch.WandErrichten
                ? GetMaxWändeErrichten(zauberer.Klasse)
                : GetMaxWändeEinreissen(zauberer.Klasse);
            if (schonGezaubert >= erlaubt)
                return Result.Fail($"{zauberer.Bezeichner} hat sein Pensum an Wänden erreicht",
                    $"Als {zauberer.Klasse} kann er {erlaubt} Wand{(erlaubt > 1 ? "e" : string.Empty)} pro Monat "
                    + $"{(spruch == Zauberspruch.WandErrichten ? "errichten" : "einreissen")}, "
                    + $"aufgegeben sind schon {schonGezaubert}.");

            if (GetAufgegebeneSprüche(zauberer).Any(befehl => befehl.Ziel.Equals(feld) && befehl.Richtung == richtung))
                return Result.Fail("Diese Gemarkseite ist schon belegt",
                    $"Für die Seite im {richtung} von {feld.CreateBezeichner()} liegt bereits ein Befehl vor.");

            kosten = BerechneWandkosten(spruch, entfernung);
            var kraft = PrüfeZauberkraft(zauberer, kosten);
            if (kraft.HasErrors)
                return kraft;

            string tätigkeit = spruch == Zauberspruch.WandErrichten ? "errichtet" : "reisst";
            string ende = spruch == Zauberspruch.WandErrichten ? string.Empty : " ein";
            return Result.Success(
                $"{zauberer.Bezeichner} {tätigkeit} die magische Wand im {richtung} von {feld.CreateBezeichner()}{ende}",
                $"Die Entfernung beträgt {entfernung} Gemark{(entfernung == 1 ? string.Empty : "en")}, "
                + $"der Spruch kostet {kosten} Zauberkraftpunkte.");
        }

        /// <summary>
        /// Prüft einen Bann, ohne etwas zu verändern.
        ///
        /// Gebannt wird auf eine Gemark in ein oder zwei Gemarken Entfernung; der Zauberer bestimmt
        /// selbst, wieviel Zauberkraft er einsetzt (Regelwerk 1.4.3).
        /// </summary>
        /// <param name="kosten">die ermittelten Kosten in ZKP, wenn die Prüfung durchgeht</param>
        public static Result PrüfeBann(Zauberer? zauberer, KleinfeldPosition? ziel, int raumpunkte, out int kosten) {
            kosten = 0;
            var grund = PrüfeGrundbedingungen(zauberer, Zauberspruch.Bannen);
            if (grund.HasErrors || zauberer == null)
                return grund;

            if (ziel == null)
                return Result.Fail("Es ist keine Gemark angegeben", "Ein Bann braucht ein Zielfeld.");
            if (KleinfeldView.GetKleinfeld(ziel) == null)
                return Result.Fail("Das Feld liegt nicht auf der Karte", ziel.CreateBezeichner());

            if (raumpunkte <= 0)
                return Result.Fail("Es wurden keine Raumpunkte angegeben",
                    $"Angegeben waren {raumpunkte} Raumpunkte.");

            int entfernung = GetEntfernung(zauberer, ziel, MaxEntfernungBann);
            if (entfernung < MinEntfernungBann)
                return Result.Fail($"{ziel.CreateBezeichner()} lässt sich von hier nicht bannen",
                    $"Gebannt wird in ein oder zwei Gemarken Entfernung (Regelwerk 1.4.3). {zauberer.Bezeichner} "
                    + $"steht auf {GetStandort(zauberer).CreateBezeichner()}"
                    + (entfernung == 0 ? "; das eigene Feld lässt sich nicht bannen." : "."));

            kosten = BerechneBannkosten(entfernung, raumpunkte);
            var kraft = PrüfeZauberkraft(zauberer, kosten);
            if (kraft.HasErrors) {
                int möglich = BerechneBannwirkung(entfernung, zauberer.GP_akt);
                return Result.Fail($"{zauberer.Bezeichner} hat nicht genug Zauberkraft",
                    $"{raumpunkte} Raumpunkte auf {entfernung} Gemark{(entfernung == 1 ? string.Empty : "en")} "
                    + $"kosten {kosten} Zauberkraftpunkte, verfügbar sind {zauberer.GP_akt}. Damit lassen sich "
                    + $"höchstens {möglich} Raumpunkte bannen.");
            }

            return Result.Success($"{zauberer.Bezeichner} bannt {raumpunkte} Raumpunkte auf {ziel.CreateBezeichner()}",
                $"Die Entfernung beträgt {entfernung} Gemark{(entfernung == 1 ? string.Empty : "en")}, "
                + $"der Bann kostet {kosten} Zauberkraftpunkte. Steht auf der Gemark ein fremder Zauberer, "
                + "kommt es zum Zauberduell (Regelwerk 1.4.3).");
        }

        /// <summary>
        /// Prüft die Forderung zu einem Zauberduell, ohne etwas zu verändern.
        ///
        /// Das Duell kostet keine Zauberkraft und ist kein Zauberspruch - es kommt zustande, wenn
        /// Zauberer verfeindeter Reiche in der gleichen oder in benachbarten Gemarken stehen
        /// (Regelwerk 5.3). Der Befehl hält fest, auf welche Gemark der Zauberer es anlegt.
        /// </summary>
        public static Result PrüfeDuell(Zauberer? zauberer, KleinfeldPosition? ziel) {
            if (zauberer == null)
                return Result.Fail("Es ist kein Zauberer ausgewählt", "Ohne Zauberer gibt es kein Duell.");
            if (zauberer.Nation != ProgramView.SelectedNation)
                return Result.Fail($"{zauberer.Bezeichner} gehört nicht zum eigenen Reich",
                    "Befehle lassen sich nur für eigene Figuren geben.");
            if (ZugView.KannBewegen == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht gefordert",
                    "Das Duell gehört zum Spielzug; erst wenn die Rüstphase abgeschlossen ist, kann gefordert werden.");
            if (ziel == null)
                return Result.Fail("Es ist keine Gemark angegeben", "Eine Forderung braucht ein Zielfeld.");
            if (KleinfeldView.GetKleinfeld(ziel) == null)
                return Result.Fail("Das Feld liegt nicht auf der Karte", ziel.CreateBezeichner());

            int entfernung = GetEntfernung(zauberer, ziel, MaxEntfernungDuell);
            if (entfernung < 0)
                return Result.Fail($"{ziel.CreateBezeichner()} liegt zu weit weg",
                    $"Zum Zauberduell kommt es, wenn Zauberer verfeindeter Reiche in der gleichen oder in "
                    + $"benachbarten Gemarken stehen (Regelwerk 5.3). {zauberer.Bezeichner} steht auf "
                    + $"{GetStandort(zauberer).CreateBezeichner()}.");

            if (Zauberbefehl.LiesAlle(zauberer.Befehl_magie)
                    .Any(befehl => befehl.Spruch == Zauberspruch.Zauberduell && befehl.Ziel.Equals(ziel)))
                return Result.Fail("Diese Forderung steht schon", $"{zauberer.Bezeichner} fordert bereits auf {ziel.CreateBezeichner()}.");

            int wirksam = GetWirksameZauberkraft(zauberer);
            string hinweis = wirksam < zauberer.GP_akt
                ? $" Achtung: {zauberer.Bezeichner} ist neutralisiert und tritt mit {wirksam} statt {zauberer.GP_akt} Zauberkraftpunkten an."
                : string.Empty;

            return Result.Success($"{zauberer.Bezeichner} fordert auf {ziel.CreateBezeichner()} zum Zauberduell",
                $"Gewürfelt wird mit {wirksam} W6 aus der wirksamen Zauberkraft; die übrigen "
                + $"{Math.Max(0, zauberer.GP_ges - wirksam)} Punkte zählen als automatische Einsen "
                + $"(Errata 31 zu Regelwerk 5.3).{hinweis}");
        }

        /// <summary>
        /// Prüft eine Teleportation mit Rüstgütern, ohne etwas zu verändern.
        ///
        /// Anders als die Teleportation des Zauberers allein ist das ein Zauberspruch: er kostet
        /// Zauberkraft und verkürzt die Reichweite (Regelwerk 1.4.1). Beherrscht wird er erst ab
        /// Klasse ZE.
        /// </summary>
        /// <param name="ladung">die Figuren, die mitgenommen werden</param>
        /// <param name="kosten">die ermittelten Kosten in ZKP, wenn die Prüfung durchgeht</param>
        public static Result PrüfeTeleportMitRüstgütern(Zauberer? zauberer, KleinfeldPosition? ziel,
                                                        IEnumerable<Spielfigur>? ladung, out int kosten) {
            kosten = 0;
            var grund = PrüfeGrundbedingungen(zauberer, Zauberspruch.TeleportMitRüstgütern);
            if (grund.HasErrors || zauberer == null)
                return grund;

            if (GetKlassenstufe(zauberer.Klasse) < GetKlassenstufe(MindestklasseTeleportMitRüstgütern))
                return Result.Fail($"{zauberer.Bezeichner} beherrscht diesen Spruch nicht",
                    $"Die Teleportation mit Rüstgütern beherrschen erst Zauberer der Klasse "
                    + $"{MindestklasseTeleportMitRüstgütern} und aufwärts (Regelwerk 1.4.1); "
                    + $"{zauberer.Bezeichner} ist {zauberer.Klasse}.");

            if (ziel == null)
                return Result.Fail("Es ist kein Zielfeld angegeben", "Ein Teleport braucht ein Ziel.");
            var zielfeld = KleinfeldView.GetKleinfeld(ziel);
            if (zielfeld == null)
                return Result.Fail("Das Zielfeld liegt nicht auf der Karte", ziel.CreateBezeichner());

            var figuren = ladung?.ToList() ?? [];
            if (figuren.Count == 0)
                return Result.Fail("Es sind keine Rüstgüter angegeben",
                    "Ohne Ladung ist das kein Zauberspruch, sondern die gewöhnliche Teleportation eines "
                    + "Zauberers (Regelwerk 1.3).");

            foreach (var figur in figuren) {
                if (figur.Nation != ProgramView.SelectedNation)
                    return Result.Fail($"{figur.Bezeichner} gehört nicht zum eigenen Reich",
                        "Es können nur reichseigene Rüstgüter teleportiert werden (Regelwerk 1.4.1).");
                if (GetStandort(zauberer).Equals(new KleinfeldPosition(
                        figur.gf_nach > 0 ? figur.gf_nach : figur.gf_von,
                        figur.gf_nach > 0 ? figur.kf_nach : figur.kf_von)) == false)
                    return Result.Fail($"{figur.Bezeichner} steht nicht beim Zauberer",
                        $"Teleportiert wird, was auf {GetStandort(zauberer).CreateBezeichner()} steht.");
            }

            int raumpunkte = figuren.Sum(figur => figur.rp);
            int transportkapazität = GetTransportkapazität(zauberer);
            if (transportkapazität > 0 && raumpunkte > transportkapazität)
                return Result.Fail($"{zauberer.Bezeichner} kann soviel nicht tragen",
                    $"Die Ladung hat {raumpunkte} Raumpunkte, die Transportkapazität der Klasse "
                    + $"{zauberer.Klasse} beträgt {transportkapazität} (Regelwerk 1.3).");

            int reichweite = BerechneTeleportreichweite(zauberer, raumpunkte);
            int entfernung = GetEntfernung(zauberer, ziel, Math.Max(reichweite, 1));
            if (entfernung <= 0)
                return Result.Fail($"{ziel.CreateBezeichner()} liegt ausserhalb der Reichweite",
                    $"Mit {raumpunkte} Raumpunkten Ladung reicht {zauberer.Bezeichner} noch {reichweite} "
                    + $"Gemark{(reichweite == 1 ? string.Empty : "en")}: die Teleportweite der Klasse "
                    + $"{zauberer.Klasse} beträgt {zauberer.MaxTeleportPunkte}, davon sind {zauberer.tp} "
                    + $"verbraucht, und je angefangene {RaumpunkteProTeleportschritt} Raumpunkte kostet die "
                    + "Ladung eine weitere Gemark (Regelwerk 1.4.1).");
            if (entfernung > reichweite)
                return Result.Fail($"{ziel.CreateBezeichner()} liegt ausserhalb der Reichweite",
                    $"Das Ziel ist {entfernung} Gemarken entfernt, mit dieser Ladung reicht der Spruch {reichweite}.");

            kosten = BerechneTeleportkosten(raumpunkte, entfernung);
            var kraft = PrüfeZauberkraft(zauberer, kosten);
            if (kraft.HasErrors)
                return kraft;

            return Result.Success($"{zauberer.Bezeichner} teleportiert {figuren.Count} Einheiten nach {ziel.CreateBezeichner()}",
                $"{raumpunkte} Raumpunkte über {entfernung} Gemark{(entfernung == 1 ? string.Empty : "en")} "
                + $"kosten {kosten} Zauberkraftpunkte.");
        }

        /// <summary>
        /// Prüft die Teleportation eines Zauberers ohne Ladung, ohne etwas zu verändern.
        ///
        /// Das ist kein Zauberspruch: "Nur die Teleportation ist eine Grundfähigkeit aller Zauberer
        /// und kostet keine Zauberkraftpunkte. Sie ist eine Bewegungsfertigkeit!!!" (Regelwerk 1.3)
        /// Sie kostet also weder Zauberkraft noch zählt sie gegen den einen Spruch pro Monat, und
        /// auch mehrere Zauberer auf einer Gemark hindern nicht daran.
        ///
        /// Bezahlt wird mit Teleportpunkten: "Zauberer bewegen sich einzeln wie Reiter und können
        /// zusätzlich einmal pro Monat gemäss nachstehender Tabelle teleportieren" - je nach Klasse
        /// 6 bis 9 Gemarken im Monat. Die Weite steht in crossref_zauberer_teleport, verbraucht
        /// wird sie in der Spalte tp.
        /// </summary>
        /// <param name="entfernung">die ermittelte Entfernung in Gemarken, wenn die Prüfung durchgeht</param>
        public static Result PrüfeTeleport(Zauberer? zauberer, KleinfeldPosition? ziel, out int entfernung) {
            entfernung = -1;
            if (zauberer == null)
                return Result.Fail("Es ist kein Zauberer ausgewählt", "Ohne Zauberer gibt es keinen Teleport.");
            if (zauberer.Nation != ProgramView.SelectedNation)
                return Result.Fail($"{zauberer.Bezeichner} gehört nicht zum eigenen Reich",
                    "Befehle lassen sich nur für eigene Figuren geben.");
            if (ZugView.KannBewegen == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht teleportiert",
                    "Der Teleport ist eine Bewegungsfertigkeit; erst wenn die Rüstphase abgeschlossen ist, "
                    + "kann bewegt werden.");
            if (ziel == null)
                return Result.Fail("Es ist kein Zielfeld angegeben", "Ein Teleport braucht ein Ziel.");

            var zielfeld = KleinfeldView.GetKleinfeld(ziel);
            if (zielfeld == null)
                return Result.Fail("Das Zielfeld liegt nicht auf der Karte", ziel.CreateBezeichner());

            var standort = GetStandort(zauberer);
            if (standort.Equals(ziel))
                return Result.Fail("Der Zauberer steht schon dort", standort.CreateBezeichner());

            int frei = GetFreieTeleportpunkte(zauberer);
            if (frei <= 0)
                return Result.Fail($"{zauberer.Bezeichner} hat seine Teleportweite verbraucht",
                    $"Als {zauberer.Klasse} kann er {zauberer.MaxTeleportPunkte} Gemarken im Monat teleportieren, "
                    + $"verbraucht sind schon {zauberer.tp}.");

            entfernung = GetEntfernung(zauberer, ziel, frei);
            if (entfernung < 0)
                return Result.Fail($"{ziel.CreateBezeichner()} liegt ausserhalb der Teleportweite",
                    $"{zauberer.Bezeichner} steht auf {standort.CreateBezeichner()} und kann in diesem Monat "
                    + $"noch {frei} Gemark{(frei == 1 ? string.Empty : "en")} weit teleportieren "
                    + $"(Klasse {zauberer.Klasse}: {zauberer.MaxTeleportPunkte} im Monat, davon {zauberer.tp} verbraucht).");

            // "Zauberer, die keine Charaktere sind, müssen sich ausserhalb des eigenen oder
            // alliierten Reichsgebietes immer bei eigenen Heeren aufhalten" (Regelwerk 1.3).
            // Wer mit wem alliiert ist, steht nicht in den Zugdaten - deshalb wird das nur als
            // Hinweis mitgegeben und nicht abgewiesen.
            string hinweis = string.Empty;
            if (zauberer.Typ != FigurType.CharakterZauberer
                && zielfeld.Nation != ProgramView.SelectedNation
                && SpielfigurenView.GetSpielfiguren(ziel).Any(f => f.Nation == ProgramView.SelectedNation) == false)
                hinweis = " Achtung: auf dem Zielfeld steht kein eigenes Heer. Ausserhalb des eigenen oder "
                    + "alliierten Reichsgebietes muss sich ein Zauberer, der kein Charakter ist, immer bei "
                    + "eigenen Heeren aufhalten (Regelwerk 1.3).";

            return Result.Success($"{zauberer.Bezeichner} teleportiert nach {ziel.CreateBezeichner()}",
                $"Das sind {entfernung} Gemark{(entfernung == 1 ? string.Empty : "en")}; danach bleiben "
                + $"{frei - entfernung} von {zauberer.MaxTeleportPunkte} übrig. Der Teleport kostet keine "
                + $"Zauberkraft (Regelwerk 1.3).{hinweis}");
        }

        /// <summary>
        /// Die Transportkapazität eines Zauberers in Raumpunkten: "A-D = 0 Raumpunkte,
        /// E-F = 40.000 Raumpunkte" (Errata 49 zu Regelwerk 1.3).
        /// </summary>
        public const int TransportkapazitätAbZE = 40000;

        public static int GetTransportkapazität(Zauberer zauberer)
            => GetKlassenstufe(zauberer.Klasse) >= GetKlassenstufe(Zaubererklasse.ZE) ? TransportkapazitätAbZE : 0;
    }
}
