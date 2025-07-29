using System;
using System.Threading.Tasks;

namespace TokeroHomeWork.Application.Interfaces
{
    public interface ICryptoPricingRepository
    { 
        Task<decimal> GetHistoricalPriceAsync(string coinId, DateTime date);
        Task<decimal?> GetCurrentPriceAsync(string coinId, string currency);
    }
}
