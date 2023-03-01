using System;
using System.IO;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filePath = "data.csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                Console.ReadLine();
                return;
            }

            try
            {
                var reader = new DataReader();
                var data = reader.Import(filePath);
                reader.PrintImportedData(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadLine();
            }
        }
    }
}