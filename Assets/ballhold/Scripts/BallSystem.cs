using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace BallHold
{
	public class BallSystem : ComponentSystem
	{
		public const float BallRadius = 22;
		private const float Gacc = 1000f;
		//private const float Gacc = 0f;
		private const float WallX = 270f - BallRadius;
		private const float TopY = 480f - 100 - BallRadius;
		//private const float TopY = 280f - BallRadius;
		private const float BoundRate = 0.4f;

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
			bool isPause = false;

			Entities.ForEach( ( ref GameMngr mngr ) => {
				isPause = mngr.IsPause;
			} );

			if( isPause )
				return;

			float dt = World.TinyEnvironment().frameDeltaTime;
			int score = 0;
			int reqEffScore = 0;
			int ballCnt = 0;

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

			// タッチ判定用.
			float2 ballRect = new float2( BallRadius*2.6f, BallRadius*2.6f );

			//Debug.LogFormatAlways("y {0} sy {1} r {2} btm {3}", boxPos.y, boxSize.y, BallRadius, boxInsideBottom);

			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.IsActive || !ball.Initialized )
					return;

				ballCnt++;

				float3 pos = float3.zero;

				switch( ball.Status ) {
				case StNorm:
					if( isHit )
						break;

					if( !ball.IsTouched && mouseOn ) {
						float3 mypos = trans.Value;
						float3 mousePos = inputSystem.GetWorldInputPosition();

						//if( isInsideCircle( mousePos, mypos, BallRadius * 1.5f ) ) {
						if( isInsideRect( mousePos, mypos, ballRect ) ) {
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
							float spd = len / delta * 0.7f;
							//Debug.LogFormatAlways( "spd {0} t {1} d {2}", spd, delta, len );

							spd = math.clamp( spd, 750f, 1500f );

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

					bool bWallHit = false;
					// 壁とのあたり.
					if( pos.x < -WallX ) {
						pos.x = -WallX;
						ball.Vx *= -BoundRate;
						trans.Value = pos;
						bWallHit = true;
					}
					else if( pos.x > WallX ) {
						pos.x = WallX;
						ball.Vx *= -BoundRate;
						trans.Value = pos;
						bWallHit = true;
					}

					// 天井とのあたり.
					if( pos.y > TopY ) {
						pos.y = TopY;
						ball.Vy *= -BoundRate;
						trans.Value = pos;
						bWallHit = true;
					}

					// 籠とのあたり.
					if( !bWallHit ) {
						float3 intersectPos = pos;
						if( isInsideBox( pos, boxPos, boxSizeR, boxSize, trans.Value, ref ball ) ) {
							float3 prePos = trans.Value;
							/*if( ball.UseOldPos ) {
								ball.UseOldPos = false;
								prePos = ball.OldPos;
								Debug.LogFormatAlways("oldpos {0}", prePos);
							}*/
							
							int hitType = IntersectCheck( prePos, pos, boxPos, boxSizeR, out intersectPos );
							if( hitType == 1 || hitType == 2 ) {
								// 左右.
								ball.Vx *= -BoundRate;
							}
							else if( hitType == 3 ) {
								// 底.
								ball.Vy *= -BoundRate;
							}
							else if( hitType == 4 ) {
								// 入った.
								ball.Status = StIn;
								ball.Timer = 0;
								// エフェクト.
								reqEffScore++;
							}
							else {
								// 籠ワープ後にちょうど籠に入ったら消す.
								ball.Timer = 3f;
							}
						}
						trans.Value = intersectPos;
					}

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
						ball.Vx *= -BoundRate;
					}
					else if( pos.x > boxInsideRight ) {
						pos.x = boxInsideRight;
						ball.Vx *= -BoundRate;
					}

					if( pos.y < boxInsideBottom ) {
						pos.y = boxInsideBottom;
					}

					trans.Value = pos;

					ball.Timer += dt;
					if( ball.Timer > 0.4f ) {
						// score.
						score += 100;

						ball.Status = StEnd;
						ball.IsActive = false;
						scl.Value.x = 0;
					}
					break;
				}

				ball.OldPos = trans.Value;

			} );

			// スコア更新.
			if( score > 0 ) {
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.IsUpdatedScore = true;
					mngr.Score += score;
				} );
			}

			// エフェクト.
			for( int i = 0; i < reqEffScore; ++i ) {
				bool recycled = false;

				Entities.ForEach( ( Entity entity, ref EffScoreInfo eff ) => {
					if( !recycled ) {
						if( !eff.IsActive ) {
							eff.IsActive = true;
							eff.Initialized = false;
							recycled = true;
						}
					}
				} );

				//Debug.LogFormatAlways( "bulcnt {0} recycled {1}", bulCnt, recycled );

				if( !recycled ) {
					var env = World.TinyEnvironment();
					SceneService.LoadSceneAsync( env.GetConfigData<GameConfig>().ScoreScn );
				}
			}

			// 現在の玉の数.
			Entities.ForEach( ( Entity entity, ref BallGenerateInfo gen ) => {
				gen.BallNum = ballCnt;
			} );

			//if( DebSpeed > 0 ) {
				Entities.WithAll<DebTextTab>().ForEach( ( Entity entity ) => {
#if false
					// float.ToString()がwebビルドで使えない.
					int i = (int)DebSpeed;
					float f = DebSpeed - (int)i;
					int fi = (int)(f * 1000f);
					string str = i.ToString() + "." + fi.ToString();
					//Debug.LogAlways( str );
#endif
					string str = ballCnt.ToString();
					EntityManager.SetBufferFromString<TextString>( entity, str );
				} );
			//}
		}


		bool isInsideCircle( float3 inputPosition, float3 position, float radius )
		{
			float distsq = math.distancesq( position.xy, inputPosition.xy );
			return distsq <= (radius * radius);
		}

		bool isInsideRect( float3 inputPosition, float3 center, float2 size )
		{
			var rect = new Rect( center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y );
			return rect.Contains( inputPosition.xy );
		}

		bool isInsideBox( float3 inputPosition, float3 center, float2 sizeR, float2 size, float3 oldPos, ref BallInfo ball )
		{
			var rect = new Rect( center.x - sizeR.x * 0.5f, center.y - sizeR.y * 0.5f, sizeR.x, sizeR.y );
			if( rect.Contains( inputPosition.xy ) ) {
#if false
				// 四隅とチェック.
				float distSq = 0;
				// A
				var rectA = new Rect( center.x - sizeR.x*0.5f, center.y + size.y*0.5f, BallRadius, BallRadius );
				if( rectA.Contains( inputPosition.xy ) ) {
					float3 posA = new float3( center.x - size.x * 0.5f, center.y + size.y * 0.5f, 0 );
					distSq = math.distancesq( inputPosition, posA );
					if( distSq > BallRadius * BallRadius ) {
						ball.UseOldPos = true;
						ball.OldPos = oldPos;
						return false;
					}
					else
						return true;
				}
				// B
				var rectB = new Rect( center.x - sizeR.x * 0.5f, center.y - sizeR.y * 0.5f, BallRadius, BallRadius );
				if( rectB.Contains( inputPosition.xy ) ) {
					float3 posB = new float3( center.x - size.x * 0.5f, center.y - size.y * 0.5f, 0 );
					distSq = math.distancesq( inputPosition, posB );
					if( distSq > BallRadius * BallRadius ) {
						Debug.LogAlways("near corner ");
						ball.UseOldPos = true;
						ball.OldPos = 0.5f*(oldPos + inputPosition);
						return false;
					}
					else
						return true;
				}
				// C
				var rectC = new Rect( center.x + size.x * 0.5f, center.y - sizeR.y * 0.5f, BallRadius, BallRadius );
				if( rectC.Contains( inputPosition.xy ) ) {
					float3 posC = new float3( center.x + size.x * 0.5f, center.y - size.y * 0.5f, 0 );
					distSq = math.distancesq( inputPosition, posC );
					if( distSq > BallRadius * BallRadius ) {
						ball.UseOldPos = true;
						ball.OldPos = oldPos;
						return false;
					}
					else
						return true;
				}
				// D
				var rectD = new Rect( center.x + size.x * 0.5f, center.y + size.y * 0.5f, BallRadius, BallRadius );
				if( rectD.Contains( inputPosition.xy ) ) {
					float3 posD = new float3( center.x + size.x * 0.5f, center.y + size.y * 0.5f, 0 );
					distSq = math.distancesq( inputPosition, posD );
					if( distSq > BallRadius * BallRadius ) {
						ball.UseOldPos = true;
						ball.OldPos = oldPos;
						return false;
					}
					else
						return true;
				}
#endif
				return true;
			}
			return false;
		}

		// return
		// 0 : 外れ.
		// 1 : 左ヒット.
		// 2 : 右ヒット.
		// 3 : 底ヒット.
		// 4 : 上ヒット. 
		int IntersectCheck( float3 prePos, float3 newPos, float3 pos, float2 sizeR, out float3 outPos )
		{
			float boxLeft = pos.x - sizeR.x * 0.5f;
			float boxRight = pos.x + sizeR.x * 0.5f;
			float boxTop = pos.y + sizeR.y * 0.5f;
			float boxBottom = pos.y - sizeR.y * 0.5f;

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
