using Unity.Entities;
using Unity.Tiny.Debugging;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace BallHold
{
	public class GameMngrSystem : ComponentSystem
	{
		private const float GameTimeMax = 30f;
		
		protected override void OnUpdate()
		{
			bool isUpdatedScore = false;
			bool isTimeOver = false;
			int score = 0;
			int time = 0;

			Entities.ForEach( ( ref GameMngr mngr ) => {
				if( !mngr.Initialized ) {
					mngr.Initialized = true;
					mngr.IsUpdatedScore = false;
					mngr.IsPause = false;
					mngr.Score = 0;
					mngr.Timer = 0;
					isUpdatedScore = true;
					return;
				}

				if( mngr.IsPause )
					return;

				if( mngr.IsUpdatedScore ) {
					isUpdatedScore = true;
					mngr.IsUpdatedScore = false;
					// ハイスコア更新.
					if( mngr.Score > mngr.HiScore ) {
						mngr.HiScore = mngr.Score;
					}
					score = mngr.Score;
				}

				mngr.Timer += World.TinyEnvironment().frameDeltaTime;
				time = (int)(GameTimeMax - mngr.Timer);
				if( mngr.Timer > GameTimeMax ) {
					isTimeOver = true;
					mngr.IsPause = true;
				}
			} );


			// スコア.
			if( isUpdatedScore ) {
				// スコア表示.
				Entities.WithAll<TextScoreTag>().ForEach( ( Entity entity ) => {
					EntityManager.SetBufferFromString<TextString>( entity, score.ToString() );
				} );
			}

			// タイム表示.
			Entities.WithAll<TextTimeTag>().ForEach( ( Entity entity ) => {
				EntityManager.SetBufferFromString<TextString>( entity, time.ToString() );
			} );

			if( isTimeOver ) {	
				// タイムオーバー表示.
				SceneReference timeOverScn = World.TinyEnvironment().GetConfigData<GameConfig>().TimeOverScn;
				SceneService.LoadSceneAsync( timeOverScn );
			}

		}

	}
}
