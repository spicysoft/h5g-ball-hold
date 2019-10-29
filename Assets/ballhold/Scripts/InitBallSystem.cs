using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace BallHold
{
	public class InitBallSystem : ComponentSystem
	{
		Random _random;

		protected override void OnUpdate()
		{
			Entities.ForEach( ( ref InitBallInfo info ) => {
				if( !info.Initialized ) {
					info.Initialized = true;
					int seed = World.TinyEnvironment().frameNum;
					_random.InitState( (uint)seed );
				}
			} );

			Entities.ForEach( ( ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.IsActive )
					return;
				if( ball.Initialized )
					return;

				ball.Initialized = true;
				ball.IsTouched = false;
				ball.Status = BallSystem.StNorm;
				ball.Timer = 0;
				ball.Vx = 0;
				ball.Vy = 0;
				scl.Value.x = 1f;
				ball.UseOldPos = false;

				float randx = _random.NextFloat( -245f, 245f );
				trans.Value.x = randx;

				float randy = (float)_random.NextInt( -261, -220 );
				trans.Value.y = randy;

#if false
				if( _random.NextBool() )
					trans.Value.y = -260f;
				else
					trans.Value.y = -220f;
#endif
			} );

		}
	}
}
