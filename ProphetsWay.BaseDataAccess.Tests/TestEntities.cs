namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Entity whose identifier follows the "{TypeName}Id" convention only.
	/// </summary>
	public class Company : IBaseEntity
	{
		public int CompanyId { get; set; }

		public string Name { get; set; }
	}

	/// <summary>
	/// A <see cref="Company"/> subtype, used to prove that a derived method whose declared return type is
	/// merely assignable to the entity type — not identical to it — still satisfies the convention.
	/// </summary>
	public class PremiumCompany : Company
	{
		public string Tier { get; set; }
	}

	/// <summary>
	/// Entity whose identifier follows the "Id" fallback convention only.
	/// </summary>
	public class Widget : IBaseEntity
	{
		public int Id { get; set; }
	}

	/// <summary>
	/// Entity exposing both "{TypeName}Id" and "Id"; the type-name-prefixed property must win.
	/// </summary>
	public class Gadget : IBaseEntity
	{
		public int GadgetId { get; set; }

		public int Id { get; set; }
	}

	/// <summary>
	/// Entity exposing neither "GhostId" nor "Id".
	/// </summary>
	public class Ghost : IBaseEntity
	{
		public string Label { get; set; }
	}

	/// <summary>
	/// Entity that satisfies <see cref="IBaseIdEntity{T}"/> through an <b>explicit</b> interface implementation,
	/// so its identifier is reachable only through the interface and never through the entity type itself.
	/// </summary>
	/// <remarks>
	/// <para>
	/// C# compiles an explicit implementation to a private property whose reflected name is the fully qualified,
	/// interface-prefixed form rather than <c>Id</c>. Identifier resolution is by name against the public surface,
	/// so neither <c>WraithId</c> nor <c>Id</c> matches and the entity is indistinguishable from <see cref="Ghost"/>
	/// as far as the dispatcher is concerned - despite the compiler having verified it carries an identifier.
	/// </para>
	/// <para>
	/// This is the pairing that makes the entity worth having: <see cref="Ghost"/> genuinely has no identifier,
	/// while this one does and still cannot be used with <c>Get&lt;T&gt;(object)</c>.
	/// </para>
	/// </remarks>
	public class Wraith : IBaseIdEntity<int>
	{
		int IBaseIdEntity<int>.Id { get; set; }
	}

	/// <summary>
	/// Entity with an accessible parameterless constructor that always throws.
	/// </summary>
	public class Detonator : IBaseEntity
	{
		public Detonator()
		{
			throw new EntityConstructorException("Detonator constructor refused to run.");
		}

		public int Id { get; set; }
	}

	/// <summary>
	/// Base type used to prove that a derived method taking the base type is not an exact-signature match.
	/// </summary>
	public abstract class EntityBase : IBaseEntity
	{
	}

	public class Product : EntityBase
	{
		public int ProductId { get; set; }
	}

	/// <summary>
	/// Entity whose "{TypeName}Id" property is a reference type, so a null identifier can legitimately be
	/// written to it.
	/// </summary>
	public class Ticket : IBaseEntity
	{
		public string TicketId { get; set; }
	}

	/// <summary>
	/// Entity whose "Id" fallback property is a nullable value type, so a null identifier can legitimately be
	/// written to it.
	/// </summary>
	public class Coupon : IBaseEntity
	{
		public int? Id { get; set; }
	}

	/// <summary>
	/// A value-type entity following the "{TypeName}Id" convention. The <c>new()</c> constraint on
	/// <c>Get&lt;T&gt;(object)</c> admits structs, so this is legal consumer code and the identifier must reach
	/// the derived method exactly as it does for a class.
	/// </summary>
	public struct Coin : IBaseEntity
	{
		public int CoinId { get; set; }
	}

	/// <summary>
	/// A value-type entity following the "Id" fallback, so the struct case is proven through both identifier
	/// resolution paths rather than only the type-name-prefixed one.
	/// </summary>
	public struct Token : IBaseEntity
	{
		public int Id { get; set; }
	}

	/// <summary>
	/// Entity whose identifier property exposes no setter at all, so the identifier can never be written to the
	/// probe entity.
	/// </summary>
	/// <remarks>
	/// A <c>private set</c> would not do: reflection resolves the set accessor including non-public ones and
	/// invokes it happily, so a privately settable identifier is written to successfully. Only a property with
	/// no set accessor whatsoever is genuinely unwritable.
	/// </remarks>
	public class Sprocket : IBaseEntity
	{
		public int SprocketId { get; }
	}

	/// <summary>
	/// Entity whose identifier setter exists, is accessible, and throws.
	/// </summary>
	public class Landmine : IBaseEntity
	{
		public const string SetterMessage = "Landmine identifier setter refused to run.";

		public int LandmineId
		{
			get { return 0; }
			set { throw new IdentifierAssignmentException(SetterMessage); }
		}
	}
}
