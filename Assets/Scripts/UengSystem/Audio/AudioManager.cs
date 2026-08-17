using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Entities;
using UengSystem.Managers;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace UengSystem.Audio {
	public class AudioManager : Manager<AudioManager> {

		public  List<AudioClip>              audioClips;
		public static int                           maxClips = 32;
		private Dictionary<string, AudioClip> audioClipsDictionary = new Dictionary<string, AudioClip>();

		private float volume;
		
		public  AudioSource       SourceBGM;
		public  AudioSource       SourceSFX;
		public  List<(AudioSource source, float? releaseTime)> playingSFX   = new (maxClips);
		
		private ObjectPool<AudioSource> SFXPool;
		
		public AudioClip GetClip(string clipName) {
			return audioClipsDictionary[clipName];
		}
		
		public void SetBGM(string clipName) {
			AudioClip clip = GetClip(clipName);
			SetBGM(clip);
		}
		
		public void SetBGM(AudioClip clip) {
			SourceBGM.clip   = clip;
			SourceBGM.volume = volume;
			SourceBGM.Play();
		}

		private Coroutine ChangeCoroutine;
		public void SetBGM(string clipName, float duration) {
			AudioClip clip = GetClip(clipName);
			SetBGM(clip, duration);
		}
		
		public void SetBGM(AudioClip clip, float duration) {
			if (ChangeCoroutine != null) {
				StopCoroutine(ChangeCoroutine);
				ChangeCoroutine = null;
			}
			
			ChangeCoroutine = StartCoroutine(ChangeBGM(clip, duration));
		}

		private IEnumerator ChangeBGM(AudioClip clip, float duration) {
			if (SourceBGM.isPlaying) {
				while (SourceBGM.volume > 0) {

					SourceBGM.volume -= volume * Time.deltaTime * 2 / duration;
					SourceBGM.volume =  Mathf.Clamp(SourceBGM.volume, 0, volume);

					yield return null;
				}
			}
			else {
				SourceBGM.volume = 0;
			}

			if (!clip) yield break;
			
			SourceBGM.clip = clip;
			SourceBGM.Play();
			
			while (SourceBGM.volume < volume) {

				SourceBGM.volume += volume * Time.deltaTime * 2 / duration;
				SourceBGM.volume =  Mathf.Clamp(SourceBGM.volume, 0, volume);
				
				yield return null;
			}
			
			ChangeCoroutine = null;
		}

		// SIMPLE - 2D SOUND //
		public void PlaySFX(string clipName) {
			AudioClip clip = GetClip(clipName);
			PlaySFX(clip);
		}
		
		public void PlaySFX(AudioClip clip) {
			SourceSFX.PlayOneShot(clip);
		}

		// MODIFIED OR STOPPABLE - 2D SOUND //
		public AudioSource PlaySFX(string clipName, float volume = 0.5f, float pitch = 1f, float pan = 0f, bool   loop = false) {
			AudioClip clip = GetClip(clipName);
			return PlaySFX(clip, volume, pitch, pan, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, float volume = 0.5f, float pitch = 1f, float pan = 0f, bool loop = false) {
			AudioSource source = SpawnSourceSFX(clip, volume, pitch, pan, 0, loop);
			source.Play();

			return source;
		}

		// WORLD FIXED - 3D SOUND //
		public AudioSource PlaySFX(string clipName, Vector2 position, float volume = 0.5f, float pitch = 1f, float  pan = 0f,  float   spread = 0f, bool  loop   = false) {
			AudioClip clip = GetClip(clipName);
			return PlaySFX(clip, position, volume, pitch, pan, spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, Vector2 position, float volume = 0.5f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			AudioSource source = SpawnSourceSFX(clip, volume, pitch, pan, spread, loop, position);
			source.Play();

			return source;
		}

		// OBJECT FIXED - 3D SOUND //
		public AudioSource PlaySFX(string clipName,   Transform parent,     Vector2 position, bool  isLocalPosition, float  volume = 0.5f, float     pitch = 1f, float   pan = 0f, float spread = 0f, bool   loop   = false) {
			AudioClip clip = GetClip(clipName);
			return PlaySFX(clip, parent, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, Transform parent, Vector2 position, bool isLocalPosition, float volume = 0.5f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			Vector2     worldPosition = isLocalPosition ? parent.TransformPoint(position) : position;
			AudioSource source        = SpawnSourceSFX(clip, volume, pitch, pan, spread, loop, worldPosition);
			source.transform.SetParent(parent, true);
			source.Play();
			
			return source;
		}

		private void AddSFX(AudioSource source, float? releaseTime) {
			if (playingSFX.Count == maxClips) {
				StopSFX(0);
			}

			bool isLoop = !releaseTime.HasValue;
			bool isEmpty = playingSFX.Count == 0;
			
			if (isLoop || isEmpty) {
				playingSFX.Add((source, releaseTime));
				return;
			}

			int index = 0;
			foreach ((AudioSource source, float? releaseTime) SFX in playingSFX) {
				bool isReleaseTimeShorter = SFX.releaseTime > releaseTime;
				bool isSFXLoop            = !SFX.releaseTime.HasValue;
				
				if (isSFXLoop || isReleaseTimeShorter) {
					break;
				}
				
				index++;
			}
			
			if (index == playingSFX.Count) playingSFX.Add((source, releaseTime)); // 리스트가 비었거나 마지막까지 조건에 부합하는 자리가 없는 경우
			else playingSFX.Insert(index, (source, releaseTime)); // 리스트에서 자리를 찾음
		}

		private AudioSource SpawnSourceSFX(AudioClip clip, float volume, float pitch, float pan, float spread, bool loop, Vector2? position = null) {
			AudioSource source = SFXPool.Get();

			float? releaseTime = loop ? null:Time.time + clip.length;
			
			AddSFX(source, releaseTime);
			
			source.clip      = clip;
			source.volume    = volume;
			source.pitch     = pitch;
			source.panStereo = pan;
			source.spread    = spread;
			source.loop      = loop;
			source.enabled   = true;
			
			if (!position.HasValue) return source;
			
			source.transform.position = position.Value;
			source.spatialBlend       = 1f;
			
			return source;
		}

		public void StopSFX(AudioSource source) { // 찾아주기
			int sourceIndex;
			for (sourceIndex = 0; sourceIndex < playingSFX.Count; sourceIndex++) {
				if (playingSFX[sourceIndex].source == source) break;
			}

			if (sourceIndex == playingSFX.Count) return; // 리스트 내에 없음.
			
			StopSFX(sourceIndex);
		}
		
		private void StopSFX(int sourceIndex) { // 사운드 소스 뒤처리
			(AudioSource source, float? releaseTime) SFX    = playingSFX[sourceIndex];
			AudioSource                              source = SFX.source;
			
			if (!source.enabled) return;
			if (source.isPlaying) source.Stop();
			
			playingSFX.RemoveAt(sourceIndex);
			
			source.panStereo    = 0f;
			source.spread       = 0f;
			source.spatialBlend = 0f;
			source.transform.SetParent(transform, true);
			
			source.enabled = false;
			SFXPool.Release(source);
		}
		
		private void StopSFXWithoutModifyList(int sourceIndex) { // 사운드 소스 뒤처리
			(AudioSource source, float? releaseTime) SFX    = playingSFX[sourceIndex];
			AudioSource                              source = SFX.source;
			
			if (!source.enabled) return;
			if (source.isPlaying) source.Stop();
			
			source.panStereo    = 0f;
			source.spread       = 0f;
			source.spatialBlend = 0f;
			source.transform.SetParent(transform, true);
			
			source.enabled = false;
			SFXPool.Release(source);
		}

		public void StopAllSFX() {
			if (playingSFX.Count == 0) return;
			for (int i = playingSFX.Count - 1; i >= 0; i--) {
				StopSFX(i);
			}
		}
		
		public override void Initialize() {
			foreach (AudioClip clip in audioClips) {
				audioClipsDictionary.Add(clip.name, clip);
			}
			
			volume = SourceBGM.volume;
			
			SFXPool = new ObjectPool<AudioSource>(
				createFunc: () => {
					AudioSource obj = Instantiate(instance.SourceSFX, transform);
					return obj;
				},
				actionOnGet: (obj) => { },
				actionOnRelease: (obj) => { },
				actionOnDestroy: Destroy,
				collectionCheck: true,
				defaultCapacity: 0,
				maxSize: 100
			);
			
			base.Initialize();
		}

		public override void ManagerUpdate() {
			if (playingSFX.Count == 0) return;
			
			if (!playingSFX[0].releaseTime.HasValue) return;
			if (playingSFX[0].releaseTime.Value > Time.time) return;

			int removeCount = 1;
			for (int i = 1; i < playingSFX.Count; i++) {
				(AudioSource source, float? releaseTime) SFX = playingSFX[i];
				if (!SFX.releaseTime.HasValue) break;
				if (SFX.releaseTime.Value > Time.time) break;
				removeCount++;
			}

			for (int i = 0; i < removeCount; i++) {
				StopSFXWithoutModifyList(i);
			}
			
			playingSFX.RemoveRange(0, removeCount);
		}
	}
}