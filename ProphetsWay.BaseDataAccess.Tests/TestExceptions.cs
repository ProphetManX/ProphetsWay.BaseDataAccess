using System;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Thrown from the body of a successfully located derived DAL method. Distinctive so that an assertion
	/// on the exception type cannot be satisfied by anything the reflection machinery produces on its own.
	/// </summary>
	public class DerivedMethodException : Exception
	{
		public DerivedMethodException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Thrown from an entity's parameterless constructor.
	/// </summary>
	public class EntityConstructorException : Exception
	{
		public EntityConstructorException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Thrown from the setter of an entity's identifier property. Distinctive so it cannot be confused with the
	/// <see cref="System.Reflection.TargetInvocationException"/> the reflection layer would otherwise produce.
	/// </summary>
	public class IdentifierAssignmentException : Exception
	{
		public IdentifierAssignmentException(string message)
			: base(message)
		{
		}
	}
}
