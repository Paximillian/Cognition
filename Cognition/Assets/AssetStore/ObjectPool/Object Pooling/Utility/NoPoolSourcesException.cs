using UnityEngine;
using System.Collections;
using System;

public class NoPoolSourcesException : Exception
{
	public NoPoolSourcesException() : base(string.Format("This pool has no sources!{0}Make sure to add at least one source in your pool's inspector.", Environment.NewLine))
	{	
	}
}
