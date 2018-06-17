using System;
using System.Diagnostics.Contracts;

public static class ExtentionArray
{
    public static Construction Get(this Construction[,] ob, Coord p)
    {
        Contract.Requires<ArgumentNullException>(p != null); //TODO vérifier si p est défini
        return ob[p.X, p.Y];
    }
}

