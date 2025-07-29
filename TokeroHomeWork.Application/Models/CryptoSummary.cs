namespace TokeroHomeWork.Application.Models;

public class CryptoSummary
{
    public string CryptoName { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal? AmountInBTC { get; set; }
}