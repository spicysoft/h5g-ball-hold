using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Text;

namespace BallHold
{
	public class InitEffScoreSystem : ComponentSystem
	{
		Random _random;

		protected override void OnUpdate()
		{
			// 籠情報.
			float3 boxPos = float3.zero;
			Entities.ForEach( ( Entity entity, ref BoxInfo box, ref Translation trans ) => {
				boxPos = trans.Value;
			} );


			Entities.ForEach( ( ref EffScoreInfo eff, ref Translation trans, ref NonUniformScale scl, ref Text2DStyle style ) => {
				if( !eff.IsActive )
					return;
				if( eff.Initialized )
					return;

				eff.Initialized = true;
				eff.Timer = 0;
				scl.Value.x = 1f;
				style.color.a = 1f;
				trans.Value = boxPos;
			} );

		}
	}
}
