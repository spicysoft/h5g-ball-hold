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
			bool isHit = false;
			float dt = World.TinyEnvironment().frameDeltaTime;
			

			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.IsActive || !ball.Initialized )
					return;

				switch( ball.Status ) {
				case StNorm:
					if( isHit )
						break;

					if( !ball.IsTouched && mouseOn ) {
						float3 mypos = trans.Value;
						float3 mousePos = inputSystem.GetWorldInputPosition();

						if( OverlapsObjectCollider( mypos, mousePos, 40f ) ) {
							// ヒットチェック1個だけにするため.
							isHit = true;

							ball.IsTouched = true;
							ball.MouseStPos = mousePos;
							ball.MouseStTime = World.TinyEnvironment().frameTime;
						}
					}
					if( ball.IsTouched && mouseUp ) {
						double time = World.TinyEnvironment().frameTime;
						float3 mpos = inputSystem.GetWorldInputPosition();

						float3 dv = mpos - ball.MouseStPos;
						//float len = math.distance( ball.MouseStPos.xy, mpos.xy );
						float len = math.length( dv );
						float delta = (float)(time - ball.MouseStTime);

						if( len > 50f ) {
							float spd = len / delta * 0.6f;
							Debug.LogFormatAlways( "spd {0} t {1} d {2}", spd, delta, len );

							ball.MoveSpd = spd;
							ball.MoveVec = dv / len;
							ball.IsTouched = false;
							ball.Status = StMove;
							ball.Vy = ball.MoveVec.y * spd;
						}
						else {
							ball.IsTouched = false;
						}
					}
					break;

				case StMove:
					float3 pos = trans.Value;
					pos.x += ball.MoveVec.x * ball.MoveSpd * dt;
					pos.y += ball.Vy * dt;

					ball.Vy -= 1200f * dt;

					trans.Value = pos;

					ball.Timer += dt;
					if( ball.Timer > 3f ) {
						ball.Status = StEnd;
						ball.IsActive = false;
						scl.Value.x = 0;
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
