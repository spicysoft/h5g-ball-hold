using Unity.Entities;

namespace BallHold
{
	public struct GameMngr : IComponentData
	{
		public bool Initialized;
		public bool IsUpdatedScore;
		public int Score;           // スコア.
		public int HiScore;			// ハイスコア.
	}
}
