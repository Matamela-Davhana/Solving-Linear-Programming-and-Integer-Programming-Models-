using System;
using System.IO;
using LinearProgrammingSolver.Core.IO; //importing interface in Solver.Core folder

namespace LinearProgrammingSolver.App.IO
{
    public class OutputWriter : IOutputWriter
    {
        private string _outputPath;

        public OutputWriter(string outputPath = "output.txt")
        {
            _outputPath = outputPath;
        }

        //Appends standard text to the file
        public void WriteHeader(string title)
        {
            using (StreamWriter sw = new StreamWriter(_outputPath, true))
            {
                sw.WriteLine("\n========================================");
                sw.WriteLine($"  {title.ToUpper()}");
                sw.WriteLine("========================================");
            }
        }

        //Writes the Canonical Form or single array (formatted to 3 decimals)
        public void WriteArray(string label, double[] values)
        {
            using (StreamWriter sw = new StreamWriter(_outputPath, true))
            {
                sw.Write($"{label}: ");
                foreach (double val in values)
                {
                    // The "F3" format string forces exactly 3 decimal places
                    sw.Write($"{val.ToString("F3")}  ");
                }
                sw.WriteLine();
            }
        }

        //Writes Tableau Iterations (2D Matrix formatted to 3 decimals)
        public void WriteTableau(string iterationName, double[,] tableau)
        {
            using (StreamWriter sw = new StreamWriter(_outputPath, true))
            {
                sw.WriteLine($"\n--- {iterationName} ---");

                int rows = tableau.GetLength(0);
                int cols = tableau.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        // PadRight ensures the columns line up nicely in the text file
                        sw.Write($"{tableau[i, j].ToString("F3").PadRight(10)}");
                    }
                    sw.WriteLine();
                }
            }
        }

        //Clears previous run data
        public void ClearPreviousOutput()
        {
            if (File.Exists(_outputPath))
            {
                File.WriteAllText(_outputPath, string.Empty);
            }
        }
    }
}