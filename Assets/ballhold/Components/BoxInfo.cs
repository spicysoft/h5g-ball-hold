using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct BoxInfo : IComponentData
	{
		public bool Initialized;
		public bool RandomInitialized;
		public float Width;
		public float Height;
		public int Status;
		public float Timer;
		public float WaitTime;
		public float3 TarDir;
		public float TarDist;
		public float Dist;
	}
}
