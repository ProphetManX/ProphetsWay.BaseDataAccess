namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Satisfies the convention for the entities whose identifier property cannot be written to, so the only
	/// thing that can fail is the assignment itself rather than a missing or mis-declared derived method.
	/// </summary>
	public class IdentifierAssignmentDataAccess : TestDataAccessBase
	{
		public bool GetSprocketWasCalled;
		public Sprocket SprocketProbe;

		public bool GetLandmineWasCalled;
		public Landmine LandmineProbe;

		public Sprocket Get(Sprocket probe)
		{
			GetSprocketWasCalled = true;
			SprocketProbe = probe;
			return probe;
		}

		public Landmine Get(Landmine probe)
		{
			GetLandmineWasCalled = true;
			LandmineProbe = probe;
			return probe;
		}
	}
}
