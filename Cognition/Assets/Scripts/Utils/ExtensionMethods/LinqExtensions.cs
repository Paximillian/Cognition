using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LinqExtensions
{
    //
    // Summary:
    //     Computes the sum of the sequence of UnityEngine.Vector3 values that are obtained by
    //     invoking a transform function on each element of the input sequence.
    //
    // Parameters:
    //   source:
    //     A sequence of values that are used to calculate a sum.
    //
    //   selector:
    //     A transform function to apply to each element.
    //
    // Type parameters:
    //   TSource:
    //     The type of the elements of source.
    //
    // Returns:
    //     The sum of the projected values.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     source or selector is null.
    //
    //   T:System.OverflowException:
    //     The sum is larger than System.Int32.MaxValue.
    public static Vector3 Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Vector3> selector)
    {
        Vector3 sum = Vector3.zero;

        foreach (TSource sourceObject in source)
        {
            sum += selector(sourceObject);
        }

        return sum;
    }
}