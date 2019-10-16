using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct BallGenerateInfo : IComponentData
	{
		public bool Initialized;
		public float Timer;
	}
}
