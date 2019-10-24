using Unity.Entities;

namespace BallHold
{
	public struct GameMngr : IComponentData
	{
		public bool Initialized;
		public bool IsUpdatedScore;
		public bool IsPause;
		public int Score;           // スコア.
		public int HiScore;         // ハイスコア.
		public float Timer;
	}
}
