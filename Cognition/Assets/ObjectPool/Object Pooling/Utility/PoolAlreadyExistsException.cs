using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolAlreadyExistsException : Exception
{
    public PoolAlreadyExistsException() : base(string.Format("An object pool with the given tag already exists"))
	{
    }
}
