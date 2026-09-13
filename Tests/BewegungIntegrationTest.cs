using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Tests der Bewegung gegen die echten Karten- und Zugdaten.
    ///
    /// Die Tests verändern nur die Objekte im Speicher. Geschrieben wird erst, wenn die Anwendung
    /// die <see cref="SharedData.StoreQueue"/> abarbeitet - das passiert im Testlauf nicht.
    /// </summary>
    public class BewegungIntegrationTest {

        /// <summary>
        /// Lädt Karte, PZE, Crossreferenzen und Zugdaten
        /// </summary>
        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
            TeleportRules.ResetCache();
            BewegungsRules.ResetCache();

            Assert.NotNull(SharedData.Map);
            Assert.NotNull(ProgramView.SelectedNation);
        }

        /// <summary>
        /// Lädt alle Datenbanken und liefert eine Figur des eigenen Reiches, die sich bewegen kann
        /// </summary>
        private static Spielfigur LadeUndFindeBewegbareFigur() {
            LadeAlles();

            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            Assert.NotEmpty(armee);

            foreach (var figur in armee) {
                if (figur.bp <= 0)
                    continue;
                if (BewegungsRules.GetMöglicheSchritte(figur).Count > 0)
                    return figur;
            }
            Assert.Fail($"In den Zugdaten von {ProgramView.SelectedNation?.Name} gibt es keine Figur, die sich noch bewegen kann");
            return null!;
        }

        /// <summary>
        /// Die Teleportfelder der Karte müssen genau denen aus dem Regelwerk (Kapitel 6.6.3) entsprechen.
        ///
        /// Achtung: die Tabelle Teleportpunkte der crossref.mdb führt als zweiten Tiefseepunkt 806/46.
        /// Regelwerk und Karte sagen übereinstimmend 806/47, dort steht auch tatsächlich ein
        /// Tiefsee-Einbahnpunkt. Deshalb ist die Karte die maßgebliche Quelle.
        /// </summary>
        [StaFact]
        public void TeleportfelderEntsprechenDemRegelwerk() {
            LadeAlles();
            var felder = TeleportRules.GetTeleportfelder();
            Assert.NotEmpty(felder);

            // die sechs Teleportfelder am Kartenrand
            foreach (string bezeichner in new[] { "601/9", "1001/5", "1503/29", "1011/38", "611/42", "103/45" }) {
                var position = SimpleParser.ParseLocation(bezeichner);
                Assert.NotNull(position);
                Assert.Equal(TeleportArt.Erkenfara, TeleportRules.GetArt(position));
            }

            // das Piratennest
            Assert.Equal(TeleportArt.Piratennest, TeleportRules.GetArt(SimpleParser.ParseLocation("2001/8")));

            // die beiden Tiefsee-Einbahnpunkte
            Assert.Equal(TeleportArt.Tiefsee, TeleportRules.GetArt(SimpleParser.ParseLocation("806/7")));
            Assert.Equal(TeleportArt.Tiefsee, TeleportRules.GetArt(SimpleParser.ParseLocation("806/47")));

            // 806/46 steht zwar in der Crossreferenztabelle, ist auf der Karte aber gewöhnliche Tiefsee
            Assert.Equal(TeleportArt.Keins, TeleportRules.GetArt(SimpleParser.ParseLocation("806/46")));
            Assert.False(TeleportRules.IstTeleportfeld(KleinfeldView.GetKleinfeld(SimpleParser.ParseLocation("806/46"))));

            // mehr Teleportfelder als diese neun gibt es nicht
            Assert.Equal(9, felder.Count);
        }

        /// <summary>
        /// Aufgetaucht wird auf den Wasserfeldern rund um den Teleportpunkt, nicht auf dem Punkt selbst.
        /// </summary>
        [StaFact]
        public void AuftauchfelderLiegenRundUmDenTeleportpunkt() {
            LadeAlles();

            foreach (var feld in TeleportRules.GetTeleportfelder()) {
                var auftauchfelder = TeleportRules.GetAuftauchfelder(feld.Position);
                Assert.True(auftauchfelder.Count > 0, $"Um {feld} herum gibt es kein Wasserfeld zum Auftauchen");
                Assert.True(auftauchfelder.Count <= 6, $"Um {feld} herum wurden mehr als sechs Felder gefunden");
                foreach (var auftauchfeld in auftauchfelder) {
                    Assert.True(auftauchfeld.IsWasser, $"{auftauchfeld.CreateBezeichner()} ist kein Wasser");
                    Assert.NotNull(BewegungsRules.GetRichtung(feld.Position, auftauchfeld));
                }
            }
        }

        /// <summary>
        /// Von Erkenfara aus führt der Teleport zum Nest, vom Nest aus zu den vorgegebenen Punkten.
        /// </summary>
        [StaFact]
        public void TeleportZieleFolgenDerRichtung() {
            LadeAlles();

            var vonErkenfara = TeleportRules.GetMöglicheZiele(SimpleParser.ParseLocation("601/9"));
            Assert.Single(vonErkenfara);
            Assert.Equal(TeleportArt.Piratennest, vonErkenfara[0].Art);

            // die Tiefseepunkte sind Einbahnstraßen und führen ebenfalls nur zum Nest
            var vonTiefsee = TeleportRules.GetMöglicheZiele(SimpleParser.ParseLocation("806/7"));
            Assert.Single(vonTiefsee);
            Assert.Equal(TeleportArt.Piratennest, vonTiefsee[0].Art);

            // vom Nest aus kommen die sechs Randfelder und die beiden Tiefseepunkte in Frage
            var vomNest = TeleportRules.GetMöglicheZiele(SimpleParser.ParseLocation("2001/8"));
            Assert.Equal(8, vomNest.Count);

            // das Auftauchen kostet nur auf der Reise von der Insel nach Erkenfara
            Assert.True(TeleportRules.KostetAuftauchen(TeleportArt.Piratennest));
            Assert.False(TeleportRules.KostetAuftauchen(TeleportArt.Erkenfara));
        }

        /// <summary>
        /// Die Bewegungsdaten aus der crossref.mdb müssen für jede Figur und jedes Gelände vorhanden sein,
        /// in dem sie sich überhaupt aufhalten kann.
        /// </summary>
        [StaFact]
        public void BewegungsdatenSindVollstaendig() {
            var figur = LadeUndFindeBewegbareFigur();
            var art = BewegungsRules.GetBewegungsArt(figur);
            Assert.NotEqual(BewegungsArt.Unbekannt, art);

            // das Gelände, auf dem die Figur steht, muss in ihrer Tabelle stehen
            var feld = KleinfeldView.GetKleinfeld(figur);
            Assert.NotNull(feld);
            Assert.NotNull(BewegungsRules.GetBewegungsdaten(art, feld.Gelaendetyp ?? 0));

            // die Höhenstufen kommen aus der Kriegertabelle und müssen für alle Landtypen da sein
            foreach (int gelaendetyp in new[] { 2, 3, 4, 5, 6, 7, 9 })
                Assert.NotNull(BewegungsRules.GetBewegungsdaten(BewegungsArt.Krieger, gelaendetyp));
        }

        /// <summary>
        /// Der gesuchte Weg muss lückenlos sein und genau so viel kosten, wie die Einzelschritte ergeben.
        /// </summary>
        [StaFact]
        public void GefundenerWegIstLueckenlosUndBezahlbar() {
            var figur = LadeUndFindeBewegbareFigur();
            var erreichbar = BewegungsRules.GetErreichbareFelder(figur);
            Assert.NotEmpty(erreichbar);

            // das am weitesten entfernte erreichbare Feld ist der interessanteste Testfall
            var ziel = erreichbar[erreichbar.Count - 1];
            var weg = BewegungsRules.FindeWeg(figur, ziel, out string fehler);
            Assert.True(weg != null, fehler);

            Assert.NotEmpty(weg!.Wegpunkte);
            Assert.True(weg.BPKosten <= figur.bp,
                $"Der Weg kostet {weg.BPKosten} BP, die Figur hat aber nur {figur.bp}");
            Assert.True(weg.HöhenstufenGesamt <= BewegungsRules.MaxHöhenstufenPunkte,
                $"Der Weg verbraucht {weg.HöhenstufenGesamt} Höhenstufenpunkte, erlaubt sind {BewegungsRules.MaxHöhenstufenPunkte}");

            // jeder Wegpunkt muss ein Nachbar des vorherigen sein
            KleinfeldPosition vorher = new(figur.gf, figur.kf);
            foreach (var punkt in weg.Wegpunkte) {
                Assert.True(BewegungsRules.GetRichtung(vorher, punkt) != null,
                    $"{punkt.CreateBezeichner()} ist kein Nachbar von {vorher.CreateBezeichner()}");
                vorher = punkt;
            }
            // und der letzte Wegpunkt ist das Ziel
            Assert.Equal(ziel.gf, vorher.gf);
            Assert.Equal(ziel.kf, vorher.kf);
        }

        /// <summary>
        /// Der wichtigste Test überhaupt: eine ausgeführte Bewegung muss sich exakt zurücknehmen lassen.
        /// In der Altanwendung wurden dabei Truppen verdoppelt.
        /// </summary>
        [StaFact]
        public void BewegungLaesstSichExaktZurueckNehmen() {
            var figur = LadeUndFindeBewegbareFigur();

            // Ausgangszustand merken
            int gfVorher = figur.gf;
            int kfVorher = figur.kf;
            int bpVorher = figur.bp;
            int hoehenstufenVorher = figur.hoehenstufen;
            int schrittVorher = figur.schritt;
            string routeVorher = figur.Route;
            string staerkeVorher = figur.Stärke;
            int figurenAufFeldVorher = KleinfeldView.GetKleinfeld(figur)!.Truppen.Count;

            var erreichbar = BewegungsRules.GetErreichbareFelder(figur);
            Assert.NotEmpty(erreichbar);
            var ziel = erreichbar[erreichbar.Count - 1];

            int befehleVorher = SharedData.Commands.Count;

            var ergebnis = BewegungView.BewegeZu(figur, ziel);
            Assert.False(ergebnis.HasErrors, $"{ergebnis.Title}: {ergebnis.Message}");

            // die Figur steht jetzt woanders und hat bezahlt
            Assert.True(figur.gf != gfVorher || figur.kf != kfVorher);
            Assert.True(figur.bp < bpVorher);
            Assert.True(figur.schritt > schrittVorher);
            Assert.Equal(befehleVorher + 1, SharedData.Commands.Count);

            // und wieder zurück
            var command = SharedData.Commands[SharedData.Commands.Count - 1];
            Assert.IsType<MoveCommand>(command);
            Assert.True(command.CanUndo);
            Assert.True(SharedData.Commands.Undo(command), "Das Undo der Bewegung ist fehlgeschlagen");

            Assert.Equal(gfVorher, figur.gf);
            Assert.Equal(kfVorher, figur.kf);
            Assert.Equal(bpVorher, figur.bp);
            Assert.Equal(hoehenstufenVorher, figur.hoehenstufen);
            Assert.Equal(schrittVorher, figur.schritt);
            Assert.Equal(routeVorher, figur.Route);
            Assert.Equal(befehleVorher, SharedData.Commands.Count);

            // die Figur darf sich dabei weder vermehrt noch verändert haben
            Assert.Equal(staerkeVorher, figur.Stärke);
            var feld = KleinfeldView.GetKleinfeld(figur);
            Assert.NotNull(feld);
            Assert.Equal(figurenAufFeldVorher, feld.Truppen.Count);
            Assert.Single(feld.Truppen.Where(t => t.Nummer == figur.Nummer && t.BaseTyp == figur.BaseTyp));
        }

        /// <summary>
        /// Der Platz fuer Wegpunkte richtet sich nach der Spur, nicht nach dem Zaehler schritt.
        ///
        /// In den echten Zugdaten stehen Figuren, deren x1/y1 gefuellt ist, waehrend schritt auf 0
        /// steht - so hinterlaesst es die Altanwendung (Theostelos Reiter 201 in Zug 40). Wer dem
        /// Zaehler glaubt, haelt eine volle Spur fuer leer und laesst Wege zu, deren letzte Schritte
        /// beim Speichern verlorengehen.
        /// </summary>
        [StaFact]
        public void EineVolleWegspurWirdAuchDannErkanntWennDerZaehlerLuegt() {
            var figur = LadeUndFindeBewegbareFigur();
            var erreichbar = BewegungsRules.GetErreichbareFelder(figur);
            Assert.NotEmpty(erreichbar);
            var ziel = erreichbar[erreichbar.Count - 1];

            // ohne Zutun ist der Weg zu finden
            Assert.NotNull(BewegungsRules.FindeWeg(figur, ziel, out _));

            var spur = new Bewegungsspur(figur);
            var original = new List<KleinfeldPosition>();
            for (int i = 0; i < spur.Count; i++)
                original.Add(spur[i]!);
            int schrittVorher = figur.schritt;
            try {
                // Spur randvoll, Zaehler auf Null - genau die Kombination aus den echten Daten,
                // nur auf die Spitze getrieben
                spur.Clear();
                for (int i = 0; i < spur.MaxWegpunkte; i++)
                    Assert.True(spur.Add(new KleinfeldPosition(figur.gf, figur.kf)));
                figur.schritt = 0;
                Assert.Equal(spur.MaxWegpunkte, spur.Count);

                var weg = BewegungsRules.FindeWeg(figur, ziel, out string fehler);
                Assert.True(weg == null,
                    "Bei voller Wegspur darf kein weiterer Weg mehr angeboten werden");
                Assert.Contains("Wegpunkte", fehler);
            }
            finally {
                spur.Clear();
                foreach (var punkt in original)
                    spur.Add(punkt);
                figur.schritt = schrittVorher;
            }
        }
    }
}
