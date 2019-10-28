using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace BallHold
{
	public class BoxSystem : ComponentSystem
	{
		Random _random;

		private const int StInit = 0;
		private const int StMove = 1;
		private const int StWait = 2;
		private const int StWarp = 3;


		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref BoxInfo box, ref Translation trans, ref Sprite2DRendererOptions opt ) => {
				if( !box.Initialized ) {
					box.Initialized = true;
					box.Width = opt.size.x;
					box.Height = opt.size.y;
					box.Status = StInit;
					box.Timer = 0;
					box.WaitTime = 0;
					box.TarDist = 0;
					box.Dist = 0;
					trans.Value = new float3( 210f, 10f, 0 );
					//trans.Value = new float3( 0f, -10f, 0 );

					int seed = World.TinyEnvironment().frameNum;
					_random.InitState( (uint)seed );
					//_random.InitState();
					return;
				}
				float dt = World.TinyEnvironment().frameDeltaTime;

				float3 nowPos = trans.Value;
				/*if( nowPos.y < 300f ) {
					nowPos.y += 100f * dt;
					trans.Value = nowPos;
				}*/

				switch( box.Status ) {
				case StInit:
					box.Timer += dt;
					if( box.Timer > 1f ) {
						box.WaitTime = 10f;
						box.Status = StWarp;
#if false
						box.Status = StMove;
						box.Timer = 0;
						box.TarDir = new float3( 0, 1f, 0 );
						box.TarDist = 250f;
#endif
					}
					break;

				case StMove:
					float spd = 30f * dt;
					float3 vel = box.TarDir * spd;
					float3 nxtPos = nowPos + vel;
					box.Dist += spd;

					trans.Value = nxtPos;
					if( box.Dist >= box.TarDist ) {
						box.Status = StWait;
						box.Timer = 0;
					}
					break;

				case StWait:
					box.Timer += dt;
					if( box.Timer > 1f ) {
						box.Status = StMove;
						box.Timer = 0;
						box.Dist = 0;

						nextTarget( ref box, ref trans );
					}
					break;

				case StWarp:
					box.Timer += dt;
					if( box.Timer > box.WaitTime ) {
						box.Timer = 0;
						box.Dist = 0;
						nextWarpPoint( ref box, ref trans );
						box.WaitTime = _random.NextFloat( 5f, 10f );
					}
					break;
				}
			} );
		}

		void nextWarpPoint( ref BoxInfo box, ref Translation trans )
		{
			nextTarget( ref box, ref trans );
			trans.Value += box.TarDir * box.TarDist;
			Debug.LogFormatAlways( "pos {0}", trans.Value );
		}

		void nextTarget( ref BoxInfo box, ref Translation trans )
		{
			float MaxX = 210f;
			float MinY = -100f;
			float MaxY = 180f;
			float centerY = 0.5f * (MaxY - MinY);

			float3 nowPos = trans.Value;
			// おおよその位置（象限）.
			int quadrant = 0;
			if( nowPos.x < 0 ) {
				if( nowPos.y > centerY )
					quadrant = 1;
				else
					quadrant = 2;
			}
			else {
				if( nowPos.y < centerY )
					quadrant = 3;
				else
					quadrant = 4;
			}

			// 進む方向.
			switch( quadrant ) {
			case 1:
				if( _random.NextBool() )
					box.TarDir = new float3( 1f, 0, 0 );
				else
					box.TarDir = new float3( 0, -1f, 0 );
				break;
			case 2:
				if( _random.NextBool() )
					box.TarDir = new float3( 1f, 0, 0 );
				else
					box.TarDir = new float3( 0, 1f, 0 );
				break;
			case 3:
				if( _random.NextBool() )
					box.TarDir = new float3( -1f, 0, 0 );
				else
					box.TarDir = new float3( 0, 1f, 0 );
				break;
			case 4:
				if( _random.NextBool() )
					box.TarDir = new float3( -1f, 0, 0 );
				else
					box.TarDir = new float3( 0, -1f, 0 );
				break;
			}


			// 進む距離.
			float tar = 0;
			float dist = 0;

			if( box.TarDir.x > 0 ) {
				tar = _random.NextFloat( 0, MaxX );
				dist = math.abs( tar - nowPos.x );
			}
			else if( box.TarDir.x < 0 ) {
				//tar = 0 - _random.NextFloat( 0, MaxX );
				tar = _random.NextFloat( -MaxX, 0 );
				dist = math.abs( tar - nowPos.x );
			}
			else if( box.TarDir.y < 0 ) {
				tar = _random.NextFloat( MinY, centerY );
				dist = math.abs( tar - nowPos.y );
			}
			else if( box.TarDir.y > 0 ) {
				tar = _random.NextFloat( centerY, MaxY );
				dist = math.abs( tar - nowPos.y );
			}

			box.TarDist = dist;
		}
	}
}
