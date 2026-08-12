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

	/// <summary>
	/// Thrown by <see cref="ConformingDataAccess"/> when it has been told to fail a commit or a roll back.
	/// Distinctive so that "the roll back genuinely failed" cannot be confused with an assertion failure or
	/// with the <see cref="InvalidOperationException"/> the transaction members throw of their own accord.
	/// </summary>
	public class TransactionFailureException : Exception
	{
		public TransactionFailureException(string message)
			: base(message)
		{
		}
	}
}
