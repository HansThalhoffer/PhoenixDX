using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.SpielfigurenView.SpielfigurenFilter;

namespace PhoenixModel.View;

/// <summary>
/// Vereinfacht die Nutzung von Truppensammlungen, die aus verschiedenen Klassen bestehen
/// </summary>
public static class SpielfigurenView {

    /// <summary>
    /// Holt alle Spielfiguren eines Kleinfeldes als eine Armee
    /// </summary>
    /// <param name="gem"></param>
    /// <returns></returns>
    public static Armee GetSpielfiguren(KleinfeldPosition gem) {
        Armee result = [];
        var kreaturen = SharedData.Kreaturen?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (kreaturen != null)
            result.AddRange(kreaturen);

        var krieger = SharedData.Krieger?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (krieger != null)
            result.AddRange(krieger);

        var reiter = SharedData.Reiter?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (reiter != null)
            result.AddRange(reiter);

        var schiffe = SharedData.Schiffe?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (schiffe != null)
            result.AddRange(schiffe);
        var charaktere = SharedData.Character?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (charaktere != null)
            result.AddRange(charaktere);
        var zauberer = SharedData.Zauberer?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
        if (zauberer != null)
            result.AddRange(zauberer);
        return result;
    }

    /// <summary>
    /// Holt alle Spielfiguren einer Nation als Armee
    /// </summary>
    /// <param name="nation"></param>
    /// <returns></returns>
    /// <summary>
    /// Ein Eintrag in der Figurenliste eines Kleinfeldes.
    ///
    /// Auf einem Feld koennen zwei ganz verschiedene Dinge stehen: eigene Figuren aus den
    /// Zugdaten und fremde Einheiten aus der Feindaufklaerung. Das sind getrennte Quellen -
    /// KleinFeld.Truppen kennt nur die erste, KleinFeld.Fremd nur die zweite. Wer nur eine
    /// abfragt, meldet auf einem Feld voller fremder Heere "hier steht nichts".
    /// </summary>
    /// <param name="Beschriftung">wie der Eintrag in einer Liste heisst</param>
    /// <param name="Nation">das Reich, dem die Einheit gehoert - soweit bekannt</param>
    /// <param name="Figur">die Spielfigur, oder null bei einer nur aufgeklärten fremden Einheit</param>
    /// <param name="Nummer">die Nummer der Einheit</param>
    /// <param name="Art">was es ist - bei fremden der Text der Feindaufklaerung</param>
    /// <param name="Stärke">die Stärke, soweit bekannt; bei fremden steht dort nichts</param>
    public record class Feldeintrag(string Beschriftung, Nation? Nation, Spielfigur? Figur,
                                    int Nummer, string Art, string Stärke) {
        /// <summary>Auswählen laesst sich nur, was einem gehoert</summary>
        public bool IstAuswählbar => Figur != null && Figur.Select();
    }

    /// <summary>
    /// Alles, was auf einem Kleinfeld steht: die eigenen Figuren zuerst, danach die fremden
    /// Einheiten, soweit die Feindaufklärung sie aufgedeckt hat.
    ///
    /// Fremde sind nur Beobachtungen - Reich, Art und Nummer, mehr weiss man nicht. Sie lassen
    /// sich daher anzeigen, aber nicht auswaehlen.
    /// </summary>
    public static List<Feldeintrag> GetFeldbesetzung(KleinfeldPosition gem) {
        List<Feldeintrag> result = [];
        foreach (var figur in GetSpielfigurenZurAuswahl(gem)) {
            bool eigen = figur.Nation != null && figur.Nation == ProgramView.SelectedNation;
            result.Add(new Feldeintrag(
                eigen ? $"{figur.Typ} {figur.Nummer} - {figur.Stärke}"
                      : $"{figur.Nation?.Reich}: {figur.Typ} {figur.Nummer} - {figur.Stärke}",
                figur.Nation, figur, figur.Nummer, figur.Typ.ToString(), figur.Stärke));
        }

        // Dieselbe Einheit kann in beiden Quellen stehen, wenn die Spielleitung alle Reiche
        // geladen hat. Dann gilt die Figur aus den Zugdaten - die weiss mehr.
        var bekannt = result
            .Where(eintrag => eintrag.Figur != null)
            .Select(eintrag => $"{eintrag.Nation?.Reich}/{eintrag.Figur!.Nummer}")
            .ToHashSet();

        foreach (var fremd in ExternalTables.Feinde.GetFeinde(gem)) {
            if (bekannt.Contains($"{fremd.Nation?.Reich}/{fremd.Nummer}"))
                continue;
            string notiz = string.IsNullOrWhiteSpace(fremd.Notiz) ? string.Empty : $" ({fremd.Notiz.Trim()})";
            result.Add(new Feldeintrag($"{fremd.Reich}: {fremd.Art} {fremd.Nummer}{notiz}",
                fremd.Nation, null, fremd.Nummer, fremd.Art, string.Empty));
        }
        return result;
    }

    /// <summary>
    /// Die Figuren einer Gemark in der Reihenfolge, in der man sie zur Auswahl anbietet:
    /// die eigenen zuerst, danach die fremden nach Reich.
    ///
    /// Fremde sind dabei, soweit die Feindaufklärung sie aufgedeckt hat - auswaehlen laesst sich
    /// nur, was einem gehoert, das entscheidet Spielfigur.Select. Sie wegzulassen waere aber
    /// schlechter: auf einem Feld, auf dem etwas steht, soll man sehen, was dort steht.
    /// </summary>
    public static List<Spielfigur> GetSpielfigurenZurAuswahl(KleinfeldPosition gem) {
        return [.. GetSpielfiguren(gem)
            .OrderBy(figur => figur.Nation != null && figur.Nation == ProgramView.SelectedNation ? 0 : 1)
            .ThenBy(figur => figur.Nation?.Reich ?? string.Empty)
            .ThenBy(figur => figur.Typ.ToString())
            .ThenBy(figur => figur.Nummer)];
    }

    public static Armee GetSpielfiguren(Nation? nation) {
        if (nation == null)
            return [];
        Armee result = [];
        var kreaturen = SharedData.Kreaturen?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (kreaturen != null)
            result.AddRange(kreaturen);

        var krieger = SharedData.Krieger?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (krieger != null)
            result.AddRange(krieger);

        var reiter = SharedData.Reiter?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (reiter != null)
            result.AddRange(reiter);

        var schiffe = SharedData.Schiffe?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (schiffe != null)
            result.AddRange(schiffe);
        var charaktere = SharedData.Character?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (charaktere != null)
            result.AddRange(charaktere);
        var zauberer = SharedData.Zauberer?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
        if (zauberer != null)
            result.AddRange(zauberer);
        return result;
    }


    /// <summary>
    /// erlaubt das Filtern der Figuren
    /// </summary>
    public class SpielfigurenFilter {
        public readonly FigurType FigurType = FigurType.None;
        public enum Search {
            None,
            Gold,
            Fernkampf,
            OhneBefehl,
            MitBefehlen,
        }

        public readonly Search SearchFor = Search.None;
        public SpielfigurenFilter(FigurType figurType) {
            FigurType = figurType;
        }
        public SpielfigurenFilter(Search searchFor) {
            SearchFor = searchFor;
        }
    }



    /// <summary>
    /// Holt alle Spielfiguren einer Nation als Armee
    /// </summary>
    /// <param name="nation"></param>
    /// <returns></returns>
    public static Armee GetSpielfiguren(Nation? nation, SpielfigurenFilter filter) {
        if (nation == null)
            return [];
        Armee result = [];
        if (filter.FigurType != FigurType.None) {
            switch (filter.FigurType) {
                case FigurType.Kreatur:
                    var kreaturen = SharedData.Kreaturen?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (kreaturen != null)
                        result.AddRange(kreaturen);
                    break;
                case FigurType.Krieger:
                    var krieger = SharedData.Krieger?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (krieger != null)
                        result.AddRange(krieger);
                    break;
                case FigurType.Reiter:
                    var reiter = SharedData.Reiter?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (reiter != null)
                        result.AddRange(reiter);
                    break;
                case FigurType.Schiff:
                    var schiffe = SharedData.Schiffe?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (schiffe != null)
                        result.AddRange(schiffe);
                    break;
                case FigurType.Charakter:
                    var charaktere = SharedData.Character?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (charaktere != null)
                        result.AddRange(charaktere);
                    break;
                case FigurType.Zauberer:

                    var zauberer = SharedData.Zauberer?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                    if (zauberer != null)
                        result.AddRange(zauberer);
                    break;
                default:
                    break;
            }
        }
  
        if (filter.SearchFor != Search.None) {
            // erlaubt die Filter zu kombinieren, sofern der Konstruktor des Filters das zulässt
            if (result.Count == 0)
                result = GetSpielfiguren(nation);
            switch (filter.SearchFor) {
                case Search.Fernkampf:
                    var fernkämpfer = result.Where(figur => figur.LeichteKP > 0 || figur.SchwereKP > 0).ToArray();
                    result.Clear();
                    result.AddRange(fernkämpfer);
                    break;
                case Search.Gold:
                    var kapitalisten = result.Where(figur => figur.Gold > 0).ToArray();
                    result.Clear();
                    result.AddRange(kapitalisten);
                    break;
                case Search.OhneBefehl:
                    var befehlslos = result.Where(figur => figur.HasCommands == false).ToArray();
                    result.Clear();
                    result.AddRange(befehlslos);
                    break;
                case Search.MitBefehlen:
                    var kommandiert = result.Where(figur => figur.HasCommands == true).ToArray();
                    result.Clear();
                    result.AddRange(kommandiert);
                    break;
            }
        }
        return result;

    }

    /// <summary>
    /// Hole alle Charaktere und Zauberer, die Spielernamen haben oder Spieler sein können
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static Spielfigur? GetSpielfigur(int id) {
        Spielfigur? spielfigur = null;
        if (SharedData.Krieger != null &&
            (spielfigur = SharedData.Krieger.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (SharedData.Reiter != null &&
            (spielfigur = SharedData.Reiter.FirstOrDefault(k => k.Nummer == id)) != null)            
            return spielfigur;
        if ( SharedData.Schiffe != null &&
           (spielfigur = SharedData.Schiffe.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (SharedData.Kreaturen != null &&
           (spielfigur = SharedData.Kreaturen.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (SharedData.Zauberer != null &&
           (spielfigur = SharedData.Zauberer.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (SharedData.Character != null &&
           (spielfigur = SharedData.Character.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        return null;
    }

    /// <summary>
    /// Hole alle Charaktere und Zauberer, die Spielernamen haben oder Spieler sein können
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static Spielfigur? GetSpielfigur(FigurType typ, int id) {
        // Der Typ einer Figur hängt von ihrer Ausrüstung ab: ein Kriegerheer mit Katapulten meldet
        // sich als LeichteArtillerie, eine Flotte mit Katapulten als Kriegsschiff. Daher wird sowohl
        // gegen den aktuellen Typ als auch gegen den Basistyp verglichen.
        bool Passt(Spielfigur figur) => figur.Nummer == id && (figur.Typ == typ || figur.BaseTyp == typ);

        Spielfigur? spielfigur = null;
        if (SharedData.Krieger != null && (spielfigur = SharedData.Krieger.FirstOrDefault(Passt)) != null)
            return spielfigur;
        if (SharedData.Reiter != null && (spielfigur = SharedData.Reiter.FirstOrDefault(Passt)) != null)
            return spielfigur;
        if (SharedData.Schiffe != null && (spielfigur = SharedData.Schiffe.FirstOrDefault(Passt)) != null)
            return spielfigur;
        if (SharedData.Kreaturen != null && (spielfigur = SharedData.Kreaturen.FirstOrDefault(Passt)) != null)
            return spielfigur;
        if (SharedData.Zauberer != null && (spielfigur = SharedData.Zauberer.FirstOrDefault(Passt)) != null)
            return spielfigur;
        if (SharedData.Character != null && (spielfigur = SharedData.Character.FirstOrDefault(Passt)) != null)
            return spielfigur;
        return null;
    }

    /// <summary>
    /// Hole alle Charaktere und Zauberer, die Spielernamen haben oder Spieler sein können
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static List<NamensSpielfigur> GetSpielerfiguren() {
        List<NamensSpielfigur> result = [];
        var charaktere = SharedData.Character?.Where(s => s.IsSpielerFigur == true && Plausibilität.IsValid(s));
        if (charaktere != null)
            result.AddRange(charaktere);
        var zauberer = SharedData.Zauberer?.Where(s => s.IsSpielerFigur == true && Plausibilität.IsValid(s));
        if (zauberer != null)
            result.AddRange(zauberer);
        return result;
    }

    /// <summary>
    /// eine klare Zuordnung zu einer Klasse ist hier schwierig, daher die Weiterleitung
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static bool BelongsToUser(Spielfigur figur) {
        return ProgramView.BelongsToUser(figur);
    }
}
