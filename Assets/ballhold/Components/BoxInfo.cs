using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct BoxInfo : IComponentData
	{
		public bool Initialized;
		public float Width;
		public float Height;
	}
}
