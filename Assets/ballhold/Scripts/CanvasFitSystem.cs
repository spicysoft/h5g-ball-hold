using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;

namespace BallHold
{
	public class CanvasFitSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entity canvasEntity = Entity.Null;

			Entities.ForEach( ( Entity entity, ref CanvasTag info, ref UICanvas canvas ) => {

				// ディスプレイ情報.
				var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
				float frameW = (float)displayInfo.frameWidth;
				float frameH = (float)displayInfo.frameHeight;

				// キャンバス情報.
				float matchval = 1f;
				if( frameH >= frameW )
					matchval = 0;

				canvas.matchWidthOrHeight = matchval;

				canvasEntity = entity;

			} );

			if( canvasEntity != Entity.Null ) {
				EntityManager.RemoveComponent<CanvasTag>( canvasEntity );
			}

		}
	}
}
