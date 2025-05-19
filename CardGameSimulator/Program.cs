namespace CardGameSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var Sim = new CardSimulator(1000000);
            Sim.Start();
         }
    }
}
