namespace IrrigationApp;

public class Data
{
    public Data()
    {
        Id = 0;
        Date = "";
        Parcelle = "";
        Appareil = "";
        M3d = 0;
        M3a = 0;
        Consomation = 0;
        Reseau = "";
        Commentaire = "";
    }
    public int Id { get; set; }
    public string Date { get; set; }
    public string Parcelle { get; set; }
    public string Appareil { get; set; }
    public int M3d { get; set; }
    public int M3a { get; set; }
    public int Consomation { get; set; }
    public string Reseau { get; set; }
    public string Commentaire { get; set; }
}
