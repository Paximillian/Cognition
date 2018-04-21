using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TypeExtensions
{
    public static string GetDisplayName(this Type i_Type)
    {
        string nameWithoutUnderscore = String.Join(" ", i_Type.Name.Split('_')
                                                              .Where(part => part.Length > 1)
                                                              .Select(part => char.ToUpper(part[0]) +
                                                                              part.Substring(1)));

        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            nameWithoutUnderscore = nameWithoutUnderscore.Replace(letter.ToString(), " " + letter);
        }

        return nameWithoutUnderscore;
    }
}
