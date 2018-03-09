using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitmqExchange.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Publisher("P");
            for (var i = 1; i < 10; i ++)
            {
                p.SendMessage(i, "slin test " + GenerateString(i));
            }

            System.Console.WriteLine("Wait");
            System.Console.ReadLine();
        }

        private static string GenerateString(int n)
        {
            var sb = new StringBuilder();
            for (var i =0; i < n; i ++)
            {
                sb.Append("0");
            }
            return sb.ToString();

        }
    }
}
