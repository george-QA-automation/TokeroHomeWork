using SkiaSharp;

namespace TokeroHomeWork.Application.Constants;

public static class CryptoConstants
{
    public static readonly List<string> CryptoList = new()
    { 
        "Bitcoin", 
        "Ethereum", 
        "Solana", 
        "Cardano", 
        "Tether", 
        "Dogecoin", 
        "Tron" 
    };
    
    public static Dictionary<string, SKColor> CryptoColors = new()
    {
        { "bitcoin", SKColor.Parse("#F7931A") },   // Orange
        { "ethereum", SKColor.Parse("#800080") },  // Purple
        { "solana", SKColor.Parse("#00FFA3") },    // Green
        { "cardano", SKColor.Parse("#0033AD") },   // Navy Blue
        { "tether", SKColor.Parse("#26A17B") },    // Teal
        { "dogecoin", SKColor.Parse("#C2A633") },  // Gold
        { "tron", SKColor.Parse("#EF0027") }       // Red
    };

}