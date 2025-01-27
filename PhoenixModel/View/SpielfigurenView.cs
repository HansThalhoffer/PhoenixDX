using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.SpielfigurenView.SpielfigurenFilter;

namespace PhoenixModel.View;

/// <summary>
/// Vereinfacht die Nutzung von Truppensammlungen, die aus verschiedenen Klassen bestehen
/// </summary>
public static class SpielfigurenView {

    /// <summary>
    /// TODO Berechnugn der Beweungspunkte 
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static int BerechneBewegungspunkte(Spielfigur figur) {
        //throw new NotImplementedException();
        return 0;
    }

    /// <summary>
    /// TODO Berechnung der Raumpunkte aus den Daten einer Spielfigur
    /// </summary>
    /// <param name="figur"></param>
    /// <returns></returns>
    public static int BerechneRaumpunkte(Spielfigur figur) {
        // die formeln für die RP von Charaktern und Zauberern ist fix
        if (figur is NamensSpielfigur namens) {
            if (figur is Character hero && hero.IsSpielerFigur == false) 
                    return 600;
            return namens.GP_akt * 50;
        }
        var kosten = KostenView.GetKosten(figur);
        if (kosten == null) {
            ProgramView.LogError($"Für die Figur {figur} findet sich kein Eintrag in der Kostentabelle", "Für die Berechnung der Raumpunkte muss die Figur in der Kostentabelle existieren");
            return 0;
        }
        int raumpunkte = 0;
        if (figur is TruppenSpielfigur truppe) {
            raumpunkte = kosten.Raumpunkte * truppe.staerke;
            if (truppe.hf > 0) {
                var equipmentKosten = KostenView.GetKosten("HF");
                if (equipmentKosten != null)
                    raumpunkte += truppe.hf * equipmentKosten.Raumpunkte;
            }
            if (truppe.LKP > 0) {
                var equipmentKosten = KostenView.GetKosten(truppe.BaseTyp == FigurType.Schiff?"LKS":"LKP");
                if (equipmentKosten != null)
                    raumpunkte += truppe.LKP * equipmentKosten.Raumpunkte;
            }
            if (truppe.SKP > 0) {
                var equipmentKosten= KostenView.GetKosten(truppe.BaseTyp == FigurType.Schiff ? "SKS" : "SKP");
                if (equipmentKosten != null)
                    raumpunkte += truppe.SKP * equipmentKosten.Raumpunkte;
            }
            if (truppe.Pferde > 0) {
                var equipmentKosten = KostenView.GetKosten("P");
                if (equipmentKosten != null)
                    raumpunkte += truppe.Pferde * equipmentKosten.Raumpunkte;
            }
        }
        return raumpunkte;
    }

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
        Spielfigur? spielfigur = null;
        if (typ == FigurType.Krieger && SharedData.Krieger != null &&
            (spielfigur = SharedData.Krieger.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (typ == FigurType.Reiter && SharedData.Reiter != null &&
            (spielfigur = SharedData.Reiter.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (typ == FigurType.Schiff && SharedData.Schiffe != null &&
           (spielfigur = SharedData.Schiffe.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (typ == FigurType.Kreatur && SharedData.Kreaturen != null &&
           (spielfigur = SharedData.Kreaturen.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (typ == FigurType.Zauberer && SharedData.Zauberer != null &&
           (spielfigur = SharedData.Zauberer.FirstOrDefault(k => k.Nummer == id)) != null)
            return spielfigur;
        if (typ == FigurType.Charakter && SharedData.Character != null &&
           (spielfigur = SharedData.Character.FirstOrDefault(k => k.Nummer == id)) != null)
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
