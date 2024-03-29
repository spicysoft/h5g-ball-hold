using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.UIControls;

namespace BallHold
{
	public class BtnRetrySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnRetryTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				if( pointerInteraction.clicked ) {
					btnOn = true;
				}
			} );

			if( btnOn ) {
				var env = World.TinyEnvironment();
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().ResultScn );
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().BallScn );

				// 初期化.
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.Initialized = false;
				} );

				Entities.ForEach( ( Entity entity, ref BallGenerateInfo gen ) => {
					gen.Initialized = false;
				} );

				Entities.ForEach( ( Entity entity, ref BoxInfo box ) => {
					box.Initialized = false;
				} );

#if false
				// 初期化.
				Entities.ForEach( ( ref BallInfo ball ) => {
					ball.Count = 0;
					ball.Initialized = false;
				} );

				Entities.ForEach( ( ref BatInfo bat ) => {
					bat.Initialized = false;
				} );

				Entities.ForEach( ( ref TargetGenInfo gen ) => {
					gen.Initialized = false;
				} );
#endif

			}
		}
	}
}
