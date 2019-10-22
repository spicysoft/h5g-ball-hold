using Unity.Entities;

namespace BallHold
{
	public struct EffScoreInfo : IComponentData
	{
		public bool IsActive;
		public bool Initialized;
		public float Timer;
	}
}
