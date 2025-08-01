namespace TokeroHomeWork.Application.Models;

public class InvestmentRecord
{
    public DateTime Date { get; set; }
    public decimal InvestedAmount { get; set; }
    public decimal CryptoAmount { get; set; }
    public string CryptoName { get; set; }
    public decimal CryptoPrice { get; set; }
    public decimal MonthlyValue { get; set; }
    public decimal ValueToday { get; set; }
    public decimal ROI { get; set; }
    public decimal PriceToday { get; set; }
}