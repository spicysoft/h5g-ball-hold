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
					Debug.LogFormatAlways( "seed {0}", seed );
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

				float randx = _random.NextFloat( -220f, 220f );
				trans.Value.x = randx;
				trans.Value.y = -260f;
			} );

		}
	}
}
