
namespace SkimiaOS.Core.Mathematics
{
    public static class MathExtensions
    {
        public static double RoundToNearest(this double amount, double roundTo)
        {
            double num = amount % roundTo;
            if (num < roundTo / 2.0)
            {
                amount -= num;
            }
            else
            {
                amount += roundTo - num;
            }
            return amount;
        }

        public static double RoundToNearest(this int amount, int roundTo)
        {
            int num = amount % roundTo;
            if ((double)num < (double)roundTo / 2.0)
            {
                amount -= num;
            }
            else
            {
                amount += roundTo - num;
            }
            return (double)amount;
        }
    }
}
