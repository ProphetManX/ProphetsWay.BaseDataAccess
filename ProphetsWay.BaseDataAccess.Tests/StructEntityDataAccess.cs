namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Satisfies the convention for the value-type entities and records the probe it was handed.
	/// </summary>
	/// <remarks>
	/// A struct probe cannot be told apart from "never called" by a null check, because the default value of
	/// the recording field is a fully formed entity. The <c>WasCalled</c> flags carry that distinction.
	/// </remarks>
	public class StructEntityDataAccess : TestDataAccessBase
	{
		public bool GetCoinWasCalled;
		public Coin CoinProbe;
		public Coin GetCoinResult = new Coin { CoinId = -1 };

		public bool GetTokenWasCalled;
		public Token TokenProbe;
		public Token GetTokenResult = new Token { Id = -1 };

		public Coin Get(Coin probe)
		{
			GetCoinWasCalled = true;
			CoinProbe = probe;
			return GetCoinResult;
		}

		public Token Get(Token probe)
		{
			GetTokenWasCalled = true;
			TokenProbe = probe;
			return GetTokenResult;
		}
	}
}
