using Unity.Entities;
using Unity.Tiny.Scenes;

namespace BallHold
{
	public struct GameConfig : IComponentData
	{
		public SceneReference TitleScn;
		public SceneReference MainScn;
		public SceneReference BallScn;
	}
}
