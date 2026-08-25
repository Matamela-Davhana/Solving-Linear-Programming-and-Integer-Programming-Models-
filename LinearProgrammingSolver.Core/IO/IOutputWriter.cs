using System;
using System.Collections.Generic;
using System.Text;

namespace LinearProgrammingSolver.Core.IO
{
    //interface to link to OutputWriter in Solver.App folder.
    public interface IOutputWriter
    {
        void WriteHeader(string title);
        void WriteArray(string label, double[] values);
        void WriteTableau(string iterationName, double[,] tableau);
    }
}
