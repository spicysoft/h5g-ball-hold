using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;
using Unity.Tiny.UIControls;

namespace BallHold
{
	public class BtnStartSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnStartTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				//Debug.LogAlways( "start btn" );
				if( pointerInteraction.clicked ) {
					Debug.LogAlways( "btn click" );
					btnOn = true;
				}
			} );


			if( btnOn ) {
				// タイトルシーンアンロード.
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<GameConfig>().TitleScn;
				SceneService.UnloadAllSceneInstances( panelBase );

				SceneReference mainScn = World.TinyEnvironment().GetConfigData<GameConfig>().MainScn;
				SceneService.LoadSceneAsync( mainScn );
			}
		}
	}
}
