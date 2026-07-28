namespace ElevageActifs.Web.Options;

public class DemoOptions
{
    public const string SectionName = "Demo";

    /// <summary>Affiche le bandeau rouge MODE DÉMO et le formulaire promoteur.</summary>
    public bool Enabled { get; set; } = true;

    public string PromoterName { get; set; } = "Équipe ElevageActifs / GISE";
    public string PromoterEmail { get; set; } = "commercial@elevageactifs.demo";
    public string? PromoterPhone { get; set; }
}
