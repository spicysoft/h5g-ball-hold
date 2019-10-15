using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct BallInfo : IComponentData
	{
		public bool Initialized;
		public bool IsTouched;
		public int Status;
		public float3 mouseStPos;
		public double mouseStTime;
	}
}
