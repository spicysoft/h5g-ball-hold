using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Tiny.Debugging;

namespace BallHold
{
	public class CameraSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach( ( ref CameraInfo info, ref Camera2D camera ) => {
				if( !info.Initialized ) {

					// ディスプレイ情報.
					var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
					float frameW = displayInfo.frameWidth;
					float frameH = (float)displayInfo.frameHeight;
					float frameAsp = frameH / frameW;

					// カメラ情報.
					float rectW = camera.rect.width;
					float rectH = camera.rect.height;
					float rectAsp = rectH / rectW;

					camera.halfVerticalSize = (0.5f * rectH) * frameAsp / rectAsp;

					//Debug.LogFormat( "----	halfvert {0}", camera.halfVerticalSize );

					info.Initialized = true;
					return;
				}
			} );

		}
	}
}
