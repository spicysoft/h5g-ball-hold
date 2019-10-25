using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;


namespace BallHold
{
	public class TimeOverSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool reqResult = false;

			Entities.ForEach( ( ref TimeOverInfo info ) => {
				info.Timer += World.TinyEnvironment().frameDeltaTime;
				if( info.Timer > 1.5f ) {
					reqResult = true;
					info.Timer = 0;
				}
			} );

			if( reqResult ) {
				var env = World.TinyEnvironment();
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().TimeOverScn );

				// リザルト表示.
				SceneReference resultScn = World.TinyEnvironment().GetConfigData<GameConfig>().ResultScn;
				SceneService.LoadSceneAsync( resultScn );
			}

		}
	}
}
