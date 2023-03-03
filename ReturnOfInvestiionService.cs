using Microsoft.VisualBasic;

namespace WebApp
{
    public class ReturnOfInvestiionService
    {
        public decimal Calculate()
        {
            int i = 0;
            decimal total = 0;
            decimal payPerYear = 100000;
            decimal entrance = 100;
            List<int> yearsOfInterest = new List<int>();
            decimal[] data = new decimal[5];
            

            foreach (int year in yearsOfInterest)
            {
                total += payPerYear - payPerYear * (entrance / 100);
                total += total * (year / 100);
                total -= (total / year) / 100;
                data[i] = Math.Round(total) - (payPerYear * (i + 1));
                i++;
            }

            return total;
        }
    }
}
