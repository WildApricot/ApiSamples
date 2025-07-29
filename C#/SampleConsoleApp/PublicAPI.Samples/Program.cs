namespace PublicAPI.Samples
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("enter api version (2.3 / 2):");
            var key = Console.ReadLine();
            switch (key)
            {
                case "2.3":
                    {
                        ApiV23Sample.Run();
                        break;
                    }
                case "2":
                    {
                        ApiV2Sample.Run();
                        break;
                    }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}