using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;
using Unity.Tiny.Text;

namespace BallHold
{
	public class BallSystem : ComponentSystem
	{
		public const float BallRadius = 20f;
		private const float Gacc = 1000f;
		private const float WallX = 270f - BallRadius;
		private const float TopY = 480f - BallRadius;

		public const int StNorm = 0;
		public const int StMove = 1;
		public const int StIn = 2;
		public const int StEnd = 10;

		protected override void OnUpdate()
		{
			var inputSystem = World.GetExistingSystem<InputSystem>();

			bool mouseOn = inputSystem.GetMouseButtonDown( 0 );
			bool mouseUp = inputSystem.GetMouseButtonUp( 0 );
			bool isHit = false;
			float dt = World.TinyEnvironment().frameDeltaTime;

			// 籠情報.
			float3 boxPos = float3.zero;
			float2 boxSize = float2.zero;
			Entities.ForEach( ( Entity entity, ref BoxInfo box, ref Translation trans, ref Sprite2DRendererOptions opt ) => {
				boxPos = trans.Value;
				//boxSize = new float2( box.Width + BallRadius, box.Height + BallRadius );
				boxSize = new float2( box.Width, box.Height );
			} );
			// 半径2個分足したサイズ.
			float2 boxSizeR = new float2( boxSize.x + BallRadius * 2f, boxSize.y + BallRadius * 2f );

			float DebSpeed = 0;

			// 箱の内側の情報.
			float boxInsideLeft = boxPos.x - boxSize.x * 0.5f + BallRadius;
			float boxInsideRight = boxPos.x + boxSize.x * 0.5f - BallRadius;
			float boxInsideBottom = boxPos.y - boxSize.y * 0.5f + BallRadius;

			//Debug.LogFormatAlways("y {0} sy {1} r {2} btm {3}", boxPos.y, boxSize.y, BallRadius, boxInsideBottom);

			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.IsActive || !ball.Initialized )
					return;

				float3 pos = float3.zero;

				switch( ball.Status ) {
				case StNorm:
					if( isHit )
						break;

					if( !ball.IsTouched && mouseOn ) {
						float3 mypos = trans.Value;
						float3 mousePos = inputSystem.GetWorldInputPosition();

						if( isInsideCircle( mousePos, mypos, BallRadius * 1.5f ) ) {
							// ヒットチェック1個だけにするため終了に.
							isHit = true;

							ball.IsTouched = true;
							ball.MouseStPos = mousePos;
							ball.MouseStTime = World.TinyEnvironment().frameTime;
						}
					}
					if( ball.IsTouched && mouseUp ) {
						double time = World.TinyEnvironment().frameTime;
						float delta = (float)(time - ball.MouseStTime);
						if( delta > 1f ) {
							ball.IsTouched = false;
							break;
						}

						float3 mpos = inputSystem.GetWorldInputPosition();
						//Debug.LogFormatAlways( "mx {0} my {1}", mpos.x, mpos.y );

						float3 dv = mpos - ball.MouseStPos;
						//float len = math.distance( ball.MouseStPos.xy, mpos.xy );
						float len = math.length( dv );

						if( len > 10f ) {
							float spd = len / delta * 0.6f;
							//Debug.LogFormatAlways( "spd {0} t {1} d {2}", spd, delta, len );

							spd = math.clamp( spd, 700f, 1500f );

							DebSpeed = spd;

							ball.MoveSpd = spd;
							ball.MoveVec = dv / len;
							ball.IsTouched = false;
							ball.Status = StMove;
							ball.Vx = ball.MoveVec.x * spd;
							ball.Vy = ball.MoveVec.y * spd;
							//Debug.LogFormatAlways( "vx {0} vy {1} d {2}", ball.Vx, ball.Vy, len );
						}
						else {
							ball.IsTouched = false;
						}
					}
					break;

				case StMove:
					pos = trans.Value;
					pos.x += ball.Vx * dt;
					pos.y += ball.Vy * dt;
					ball.Vy -= Gacc * dt;

					// 壁とのあたり.
					if( pos.x < -WallX ) {
						pos.x = -WallX;
						ball.Vx *= -0.5f;
						trans.Value = pos;
					}
					else if( pos.x > WallX ) {
						pos.x = WallX;
						ball.Vx *= -0.5f;
						trans.Value = pos;
					}

					// 天井とのあたり.
					if( pos.y > TopY ) {
						pos.y = TopY;
						ball.Vy *= -0.5f;
						trans.Value = pos;
					}

					// 籠とのあたり.
					float3 intersectPos = pos;
					if( isInsideBox( pos, boxPos, boxSizeR ) ) {
						int hitType = IntersectCheck( trans.Value, pos, boxPos, boxSizeR, out intersectPos );
						if( hitType == 1 || hitType == 2 ) {
							// 左右.
							ball.Vx *= -0.5f;
						}
						else if( hitType == 3 ) {
							// 底.
							ball.Vy *= -0.5f;
						}
						else if( hitType == 4 ) {
							// 入った.
							ball.Status = StIn;
							ball.Timer = 0;
						}
					}
					trans.Value = intersectPos;


					ball.Timer += dt;
					if( ball.Timer > 3f ) {
						ball.Status = StEnd;
						ball.IsActive = false;
						scl.Value.x = 0;
					}
					break;

				case StIn:
					pos = trans.Value;
					pos.x += ball.Vx * dt;
					pos.y += ball.Vy * dt;
					ball.Vy -= Gacc * dt;

					if( pos.x < boxInsideLeft ) {
						pos.x = boxInsideLeft;
						ball.Vx *= -0.5f;
					}
					else if( pos.x > boxInsideRight ) {
						pos.x = boxInsideRight;
						ball.Vx *= -0.5f;
					}

					if( pos.y < boxInsideBottom ) {
						pos.y = boxInsideBottom;
					}

					trans.Value = pos;

					ball.Timer += dt;
					if( ball.Timer > 0.5f ) {
						ball.Status = StEnd;
						ball.IsActive = false;
						scl.Value.x = 0;
					}
					break;
				}
			} );


			if( DebSpeed > 0 ) {
				Entities.WithAll<DebTextTab>().ForEach( ( Entity entity ) => {
					// float.ToString()がwebビルドで使えない.
					int i = (int)DebSpeed;
					float f = DebSpeed - (int)i;
					int fi = (int)(f * 1000f);
					string str = i.ToString() + "." + fi.ToString();
					Debug.LogAlways( str );
					EntityManager.SetBufferFromString<TextString>( entity, str );
				} );
			}
		}


		bool isInsideCircle( float3 inputPosition, float3 position, float radius )
		{
			float distsq = math.distancesq( position.xy, inputPosition.xy );
			return distsq <= (radius * radius);
		}

		bool isInsideBox( float3 inputPosition, float3 position, float2 size )
		{
			var rect = new Rect( position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y );
			return rect.Contains( inputPosition.xy );
		}

		// return
		// 0 : 外れ.
		// 1 : 左ヒット.
		// 2 : 右ヒット.
		// 3 : 底ヒット.
		// 4 : 上ヒット. 
		int IntersectCheck( float3 prePos, float3 newPos, float3 pos, float2 size, out float3 outPos )
		{
			float boxLeft = pos.x - size.x * 0.5f;
			float boxRight = pos.x + size.x * 0.5f;
			float boxTop = pos.y + size.y * 0.5f;
			float boxBottom = pos.y - size.y * 0.5f;

			float3 posA = new float3( boxLeft, boxTop, 0 );     // 左上.
			float3 posB = new float3( boxLeft, boxBottom, 0 );  // 左下.
			float3 posC = new float3( boxRight, boxBottom, 0 ); // 右下.
			float3 posD = new float3( boxRight, boxTop, 0 );    // 右上.
			float3 intersectPos = float3.zero;

			float dx = newPos.x - prePos.x;
			float dy = newPos.y - prePos.y;

			int isHit = 0;
			if( dx > 0 ) {
				if( prePos.x < boxLeft && newPos.x >= boxLeft ) {
					// 左辺交差チェック.
					if( isIntersectLine( prePos, newPos, posA, posB, out intersectPos ) ) {
						//Debug.LogAlways("left hit");
						isHit = 1;
					}
				}
			}
			else if( dx < 0 ) {
				if( prePos.x > boxRight && newPos.x <= boxRight ) {
					// 右辺交差チェック.
					if( isIntersectLine( prePos, newPos, posD, posC, out intersectPos ) ) {
						//Debug.LogAlways( "right hit" );
						isHit = 2;
					}
				}
			}

			if( dy > 0 ) {
				if( prePos.y < boxBottom && newPos.y >= boxBottom ) {
					// 底辺交差チェック.
					if( isIntersectLine( prePos, newPos, posB, posC, out intersectPos ) ) {
						//Debug.LogAlways( "bottom hit" );
						isHit = 3;

					}
				}
			}
			else if( dy < 0 ) {
				if( prePos.y > boxTop && newPos.y <= boxTop ) {
					// 上辺交差チェック.
					if( isIntersectLine( prePos, newPos, posA, posD, out intersectPos ) ) {
						//Debug.LogAlways( "top hit" );
						isHit = 4;

					}
				}
			}

			outPos = intersectPos;
			return isHit;
		}


		// 参考: https://qiita.com/Nunocky/items/55db409d90ebe0aac280
		bool isIntersectLine( float3 st1, float3 ed1, float3 st2, float3 ed2, out float3 p )
		{
			p = st1;

			float2 v1 = new float2( st1.x - st2.x, st1.y - st2.y );
			float2 vA = new float2( ed1.x - st1.x, ed1.y - st1.y );
			float2 vB = new float2( ed2.x - st2.x, ed2.y - st2.y );

			// 外積.
			float cross = vA.x * vB.y - vA.y * vB.x;

			// 外積=0(平行)なら交差しない.
			if( math.abs( cross ) < 0.00001f ) {
				return false;
			}

			float t = (v1.y * vB.x - v1.x * vB.y) / cross;
			float s = (v1.y * vA.x - v1.x * vA.y) / cross;

			if( t < 0 || t > 1f || s < 0 || s > 1f ) {
				return false;
			}

			p = new float3( vA.x * t + st1.x, vA.y * t + st1.y, 0 );
			return true;
		}
	}
}
