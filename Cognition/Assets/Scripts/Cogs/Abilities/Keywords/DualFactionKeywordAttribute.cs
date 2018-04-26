using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keywords marked by this can be used to affect only ally cogs, only enemy cogs, or both.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DualFactionKeywordAttribute : Attribute
{
}