using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;

namespace BallHold
{
	public class BallSystem : ComponentSystem
	{
		public const int StNorm = 0;
		public const int StMove = 1;
		public const int StEnd = 10;


		protected override void OnUpdate()
		{
			var inputSystem = World.GetExistingSystem<InputSystem>();

			bool mouseOn = inputSystem.GetMouseButtonDown( 0 );
			bool mouseUp = inputSystem.GetMouseButtonUp( 0 );

			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans ) => {
				// 仮.
				if( !ball.Initialized ) {
					ball.Initialized = true;
					ball.IsTouched = false;
					ball.Status = StNorm;
					return;
				}

				switch( ball.Status ) {
				case StNorm:
					if( !ball.IsTouched && mouseOn ) {
						float3 mypos = trans.Value;
						float3 mousePos = inputSystem.GetWorldInputPosition();

						if( OverlapsObjectCollider( mypos, mousePos, 40f ) ) {
							Debug.LogAlways( "hit" );
							ball.IsTouched = true;
							ball.mouseStPos = mousePos;
							ball.mouseStTime = World.TinyEnvironment().frameTime;
						}
					}
					if( ball.IsTouched && mouseUp ) {
						double time = World.TinyEnvironment().frameTime;
						float3 mpos = inputSystem.GetWorldInputPosition();
						float len = math.distance( ball.mouseStPos.xy, mpos.xy );
						float dt = (float)(time - ball.mouseStTime);

						float spd = len / dt;
						Debug.LogFormatAlways( "spd {0} t {1} d {2}", spd, dt, len );

						ball.IsTouched = false;
					}
					break;
				}
			} );

		}



		bool OverlapsObjectCollider( float3 position, float3 inputPosition, float radius )
		{
			float distsq = math.distancesq( position.xy, inputPosition.xy );
			return distsq <= (radius * radius);

			//var rect = new Rect( position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y );
			//return rect.Contains( inputPosition.xy );
		}
	}
}
