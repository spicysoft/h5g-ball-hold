using Unity.Entities;
using Unity.Tiny.Core2D;

namespace BallHold
{
	public class BoxSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref BoxInfo box, ref Translation trans, ref Sprite2DRendererOptions opt ) => {
				if( !box.Initialized ) {
					box.Initialized = true;
					box.Width = opt.size.x;
					box.Height = opt.size.y;
					return;
				}

			} );
		}
	}
}
