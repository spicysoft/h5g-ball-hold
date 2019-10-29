using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;

namespace BallHold
{
	public class BallGenerateSystem : ComponentSystem
	{
		private const int BallMax = 60;

		protected override void OnUpdate()
		{
			bool reqGen = false;
			bool isPause = false;

			Entities.ForEach( ( ref GameMngr mngr ) => {
				isPause = mngr.IsPause;
			} );
			if( isPause )
				return;


			Entities.ForEach( ( Entity entity, ref BallGenerateInfo gen ) => {
				if( !gen.Initialized ) {
					gen.Initialized = true;
					gen.Timer = 0;
					gen.BallNum = 0;
					return;
				}

				float dt = World.TinyEnvironment().frameDeltaTime;
				float interval = 0.4f;
				if( gen.BallNum < 15 )
					interval = 0.1f;
				else if( gen.BallNum < 25 )
					interval = 0.2f;
				else if( gen.BallNum > 50 )
					interval = 0.8f;

				gen.Timer += dt;
				if( gen.Timer > interval ) {
					if( gen.BallNum < BallMax )
						reqGen = true;
					gen.Timer = 0;
				}

			} );

			if( reqGen ) {
				bool recycled = false;

				Entities.ForEach( ( Entity entity, ref BallInfo ball ) => {
					if( !recycled ) {
						if( !ball.IsActive ) {
							ball.IsActive = true;
							ball.Initialized = false;
							recycled = true;
						}
					}
				} );

				//Debug.LogFormatAlways( "bulcnt {0} recycled {1}", bulCnt, recycled );

				if( !recycled ) {
					var env = World.TinyEnvironment();
					SceneService.LoadSceneAsync( env.GetConfigData<GameConfig>().BallScn );
				}
			}
		}
	}
}
