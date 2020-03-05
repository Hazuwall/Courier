using Courier;
using System;
using System.Collections.Generic;

public abstract class ModelBase
{
	public const string EmptyClassName = "Empty";
	public const string UnknownClassName = "Unknown";

	public abstract bool DoCollide { get; }
	public abstract bool IsCallable { get; }
	public abstract bool IsStatic { get; }
	public abstract string Class { get; }

	public virtual IEnumerable<IWorldAction> Call()
	{
		return null;
	}
}
