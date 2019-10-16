using Unity.Entities;
using Unity.Mathematics;

namespace BallHold
{
	public struct BallInfo : IComponentData
	{
		public bool IsActive;
		public bool Initialized;
		public bool IsTouched;
		public int Status;
		public float3 MouseStPos;
		public double MouseStTime;
		public float3 MoveVec;
		public float MoveSpd;
		public float Timer;
		public float Vy;
	}
}
