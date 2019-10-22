using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Text;

namespace BallHold
{
	public class EffScoreSystem : ComponentSystem
	{

		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref EffScoreInfo eff, ref Translation trans, ref NonUniformScale scl, ref Text2DStyle style ) => {
				if( !eff.Initialized ) {
					return;
				}
				float dt = World.TinyEnvironment().frameDeltaTime;

				float3 nowPos = trans.Value;
				nowPos.y += 160f * dt;
				trans.Value = nowPos;

				Color col = style.color;
				col.a -= dt;
				style.color = col;

				eff.Timer += dt;
				if( eff.Timer > 0.8f ) {
					eff.IsActive = false;
					scl.Value.x = 0;
				}
			} );
		}

	}
}
