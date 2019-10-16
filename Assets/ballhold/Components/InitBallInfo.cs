using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct InitBallInfo : IComponentData
	{
		public bool Initialized;
	}
}
