using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace UengSystem.Audio {
	public class AudioManager : Manager<AudioManager> {

		public  AudioClip[]                   audioClips;
		private Dictionary<string, AudioClip> audioClipsDictionary = new Dictionary<string, AudioClip>();

		private float volume;
		
		public AudioSource SourceBGM;
		public AudioSource SourceSFX;
		public List<AudioSource> playingSFX = new List<AudioSource>();

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
			AudioSource source = SpawnSourceSFX(volume, pitch, pan, 0, loop);
			
			source.clip = clip;
			
			playingSFX.Add(source);
			source.Play();
			
			if (!loop) reserveStopSFX(source);
			return source;
		}

		// WORLD FIXED - 3D SOUND //
		public AudioSource PlaySFX(string clipName, Vector2 position, float volume = 0.5f, float pitch = 1f, float  pan = 0f,  float   spread = 0f, bool  loop   = false) {
			AudioClip clip = GetClip(clipName);
			return PlaySFX(clip, position, volume, pitch, pan, spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, Vector2 position, float volume = 0.5f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			AudioSource source = SpawnSourceSFX(volume, pitch, pan, spread, loop, position);
			
			playingSFX.Add(source);
			
			source.clip = clip;
			source.Play();
			
			if (!loop) reserveStopSFX(source);
			return source;
		}

		// OBJECT FIXED - 3D SOUND //
		public AudioSource PlaySFX(string clipName,   Transform parent,     Vector2 position, bool  isLocalPosition, float  volume = 0.5f, float     pitch = 1f, float   pan = 0f, float spread = 0f, bool   loop   = false) {
			AudioClip clip = GetClip(clipName);
			return PlaySFX(clip, parent, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, Transform parent, Vector2 position, bool isLocalPosition, float volume = 0.5f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			Vector2 worldPosition = isLocalPosition ? parent.TransformPoint(position) : position;
			AudioSource source = SpawnSourceSFX(volume, pitch, pan, spread, loop, worldPosition);
			
			source.transform.SetParent(parent, true);
			playingSFX.Add(source);
			
			source.clip = clip;
			source.Play();

			if (!loop) reserveStopSFX(source);
			return source;
		}

		private AudioSource SpawnSourceSFX(float volume, float pitch, float pan, float spread, bool loop, Vector2? position = null) {
			AudioSource source = SFXPool.Get();
			
			source.volume = volume;
			source.pitch = pitch;
			source.panStereo = pan;
			source.spread = spread;
			source.loop   = loop;
			
			if (!position.HasValue) return source;
			
			source.transform.position = position.Value;
			source.spatialBlend       = 1f;
			return source;
		}

		public void StopSFX(AudioSource source) {
			if (!source.enabled) return;
			if (source.isPlaying) source.Stop();
			
			source.panStereo    = 0f;
			source.spread       = 0f;
			source.spatialBlend = 0f;
			source.transform.SetParent(transform, true);

			playingSFX.Remove(source);
			SFXPool.Release(source);
		}

		public void StopAllSFX() {
			if (playingSFX.Count == 0) return;
			for (int i = playingSFX.Count-1; i >= 0; i--) {
				AudioSource source = playingSFX[i];
				StopSFX(source);
			}
		}

		private void reserveStopSFX(AudioSource source) {
			USFXSource usfxSource = source.GetComponent<USFXSource>();
			new DelayedAction(source.clip.length, () => StopSFX(source), () => { }, usfxSource).ExecuteDA();
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

		public override void ManagerUpdate() { }
		public override void ManagerFixedUpdate() { }
	}
}