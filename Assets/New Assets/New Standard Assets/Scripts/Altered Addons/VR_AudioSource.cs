using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class VR_AudioSource : UpdateWhileEnabled
{
	public bool bypassRoomEffects = false;
	public float directivityAlpha = 0.0f;
	public float directivitySharpness = 1.0f;
	public float listenerDirectivityAlpha = 0.0f;
	public float listenerDirectivitySharpness = 1.0f;
	public float gainDb = 0.0f;
	public bool occlusionEnabled = false;
	public bool playOnAwake = true;
	public bool disableOnStop = false;
	public AudioClip Clip
	{
		get
		{
			return sourceClip;
		}
		set
		{
			sourceClip = value;
			if (audioSource != null)
				audioSource.clip = sourceClip;
		}
	}
	public AudioClip sourceClip;

	public bool IsPlaying
	{
		get
		{
			if (audioSource != null)
				return audioSource.isPlaying;
			else
				return false;
		}
	}

	public bool Loop
	{
		get
		{
			return sourceLoop;
		}
		set
		{
			sourceLoop = value;
			if (audioSource != null)
				audioSource.loop = sourceLoop;
		}
	}
	public bool sourceLoop = false;

	/// Un- / Mutes the source. Mute sets the volume=0, Un-Mute restore the original volume.
	public bool mute {
		get { return sourceMute; }
		set {
		sourceMute = value;
		if (audioSource != null) {
			audioSource.mute = sourceMute;
		}
		}
	}
	[SerializeField]
	bool sourceMute = false;

	/// The pitch of the audio source.
	public float pitch {
		get { return sourcePitch; }
		set {
		sourcePitch = value;
		if (audioSource != null) {
			audioSource.pitch = sourcePitch;
		}
		}
	}
	[SerializeField]
	[Range(-3.0f, 3.0f)]
	float sourcePitch = 1.0f;

	/// Sets the priority of the audio source.
	public int priority {
		get { return sourcePriority; }
		set {
		sourcePriority = value;
		if(audioSource != null) {
			audioSource.priority = sourcePriority;
		}
		}
	}
	[SerializeField]
	[Range(0, 256)]
	int sourcePriority = 128;

	/// Sets how much this source is affected by 3D spatialization calculations (attenuation, doppler).
	public float spatialBlend {
		get { return sourceSpatialBlend; }
		set {
		sourceSpatialBlend = value;
		if (audioSource != null) {
			audioSource.spatialBlend = sourceSpatialBlend;
		}
		}
	}
	[SerializeField]
	[Range(0.0f, 1.0f)]
	float sourceSpatialBlend = 1.0f;

	/// Sets the Doppler scale for this audio source.
	public float dopplerLevel {
		get { return sourceDopplerLevel; }
		set {
		sourceDopplerLevel = value;
		if(audioSource != null) {
			audioSource.dopplerLevel = sourceDopplerLevel;
		}
		}
	}
	[SerializeField]
	[Range(0.0f, 5.0f)]
	float sourceDopplerLevel = 1.0f;

	/// Sets the spread angle (in degrees) in 3D space.
	public float spread {
		get { return sourceSpread; }
		set {
		sourceSpread = value;
		if(audioSource != null) {
			audioSource.spread = sourceSpread;
		}
		}
	}
	[SerializeField]
	[Range(0.0f, 360.0f)]
	float sourceSpread = 0.0f;

	/// Playback position in seconds.
	public float time {
		get {
		if(audioSource != null) {
			return audioSource.time;
		}
		return 0.0f;
		}
		set {
		if(audioSource != null) {
			audioSource.time = value;
		}
		}
	}

	/// Playback position in PCM samples.
	public int timeSamples {
		get {
		if(audioSource != null) {
			return audioSource.timeSamples;
		}
		return 0;
		}
		set {
		if(audioSource != null) {
			audioSource.timeSamples = value;
		}
		}
	}

	/// The volume of the audio source (0.0 to 1.0).
	public float volume {
		get { return sourceVolume; }
		set {
		sourceVolume = value;
		if (audioSource != null) {
			audioSource.volume = sourceVolume;
		}
		}
	}
	[SerializeField]
	[Range(0.0f, 1.0f)]
	float sourceVolume = 1.0f;

	/// Volume rolloff model with respect to the distance.
	public AudioRolloffMode rolloffMode {
		get { return sourceRolloffMode; }
		set {
		sourceRolloffMode = value;
		if (audioSource != null) {
			audioSource.rolloffMode = sourceRolloffMode;
			if (rolloffMode == AudioRolloffMode.Custom) {
			// Custom rolloff is not supported, set the curve for no distance attenuation.
			audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
										AnimationCurve.Linear(sourceMinDistance, 1.0f,
															sourceMaxDistance, 1.0f));
			}
		}
		}
	}
	[SerializeField]
	AudioRolloffMode sourceRolloffMode = AudioRolloffMode.Logarithmic;

	/// MaxDistance is the distance a sound stops attenuating at.
	public float maxDistance {
		get { return sourceMaxDistance; }
		set {
		sourceMaxDistance = Mathf.Clamp(value, sourceMinDistance + VR_Audio.distanceEpsilon,
										VR_Audio.maxDistanceLimit);
		if(audioSource != null) {
			audioSource.maxDistance = sourceMaxDistance;
		}
		}
	}
	[SerializeField]
	float sourceMaxDistance = 500.0f;

	/// Within the Min distance the VR_AudioSource will cease to grow louder in volume.
	public float minDistance {
		get { return sourceMinDistance; }
		set {
		sourceMinDistance = Mathf.Clamp(value, 0.0f, VR_Audio.minDistanceLimit);
		if(audioSource != null) {
			audioSource.minDistance = sourceMinDistance;
		}
		}
	}
	[SerializeField]
	float sourceMinDistance = 1.0f;

	/// Binaural (HRTF) rendering toggle.
	[SerializeField]
	bool hrtfEnabled = true;

	// Unity audio source attached to the game object.
	[SerializeField]
	AudioSource audioSource = null;

	// Unique source id.
	int id = -1;

	// Current occlusion value;
	float currentOcclusion = 0.0f;

	// Next occlusion update time in seconds.
	float nextOcclusionUpdate = 0.0f;

	// Denotes whether the source is currently paused or not.
	bool isPaused = false;

	void Awake () {
		if (audioSource == null) {
		// Ensure the audio source gets created once.
		audioSource = gameObject.AddComponent<AudioSource>();
		}
		audioSource.enabled = false;
		audioSource.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
		audioSource.playOnAwake = false;
		audioSource.bypassReverbZones = true;
	#if UNITY_5_5_OR_NEWER
		audioSource.spatializePostEffects = true;
	#endif  // UNITY_5_5_OR_NEWER
		OnValidate();
		if (Application.platform != RuntimePlatform.Android) {
		// TODO: VR_Audio bug on Android with Unity 2017

		// Route the source output to |VR_AudioMixer|.
		AudioMixer mixer = (Resources.Load("VR_AudioMixer") as AudioMixer);
		if(mixer != null) {
			audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
		} else {
			Debug.LogError("VR_AudioMixer could not be found in Resources. Make sure that the GVR SDK " +
						"Unity package is imported properly.");
		}
		}
	}

	void OnEnable () {
		audioSource.enabled = true;
		if (playOnAwake && !IsPlaying && InitializeSource()) {
		Play();
		}
	}

	void Start () {
		if (playOnAwake && !IsPlaying) {
		Play();
		}
	}

	void OnDisable () {
		Stop();
		audioSource.enabled = false;
	}

	void OnDestroy () {
		Destroy(audioSource);
	}

	void OnApplicationPause (bool pauseStatus) {
		if (pauseStatus) {
		Pause();
		} else {
		UnPause();
		}
	}

	public override void DoUpdate ()
	{
		if (disableOnStop && !IsPlaying)
		gameObject.SetActive(false);

		// Update occlusion state.
		if (!occlusionEnabled) {
		currentOcclusion = 0.0f;
		} else if (Time.time >= nextOcclusionUpdate) {
		nextOcclusionUpdate = Time.time + VR_Audio.occlusionDetectionInterval;
		currentOcclusion = VR_Audio.ComputeOcclusion(transform);
		}
		// Update source.
		if (!IsPlaying && !isPaused)
		Stop();
		else
		{
			audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.Gain,
											VR_Audio.ConvertAmplitudeFromDb(gainDb));
			audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.MinDistance,
											sourceMinDistance);
			VR_Audio.UpdateAudioSource(id, this, currentOcclusion);
		}
	}

	/// Provides a block of the currently playing source's output data.
	///
	/// @note The array given in samples will be filled with the requested data before spatialization.
	public void GetOutputData(float[] samples, int channel) {
		if (audioSource != null) {
		audioSource.GetOutputData(samples, channel);
		}
	}

	/// Provides a block of the currently playing audio source's spectrum data.
	///
	/// @note The array given in samples will be filled with the requested data before spatialization.
	public void GetSpectrumData(float[] samples, int channel, FFTWindow window) {
		if (audioSource != null) {
		audioSource.GetSpectrumData(samples, channel, window);
		}
	}

	/// Pauses playing the clip.
	public void Pause () {
		if (audioSource != null) {
		isPaused = true;
		audioSource.Pause();
		}
	}

	/// Plays the clip.
	public void Play () {
		if (audioSource != null && InitializeSource()) {
		audioSource.Play();
		isPaused = false;
		} else {
		Debug.LogWarning ("GVR Audio source not initialized. Audio playback not supported " +
							"until after Awake() and OnEnable(). Try calling from Start() instead.");
		}
	}

	/// Plays the clip with a delay specified in seconds.
	public void PlayDelayed (float delay) {
		if (audioSource != null && InitializeSource()) {
		audioSource.PlayDelayed(delay);
		isPaused = false;
		} else {
		Debug.LogWarning ("GVR Audio source not initialized. Audio playback not supported " +
							"until after Awake() and OnEnable(). Try calling from Start() instead.");
		}
	}

	/// Plays an AudioClip.
	public void PlayOneShot (AudioClip clip) {
		PlayOneShot(clip, 1.0f);
	}

	/// Plays an AudioClip, and scales its volume.
	public void PlayOneShot (AudioClip clip, float volume) {
		if (audioSource != null && InitializeSource()) {
		audioSource.PlayOneShot(clip, volume);
		isPaused = false;
		} else {
		Debug.LogWarning ("GVR Audio source not initialized. Audio playback not supported " +
							"until after Awake() and OnEnable(). Try calling from Start() instead.");
		}
	}

	/// Plays the clip at a specific time on the absolute time-line that AudioSettings.dspTime reads
	/// from.
	public void PlayScheduled (double time) {
		if (audioSource != null && InitializeSource()) {
		audioSource.PlayScheduled(time);
		isPaused = false;
		} else {
		Debug.LogWarning ("GVR Audio source not initialized. Audio playback not supported " +
							"until after Awake() and OnEnable(). Try calling from Start() instead.");
		}
	}

	/// Changes the time at which a sound that has already been scheduled to play will end.
	public void SetScheduledEndTime(double time) {
		if (audioSource != null) {
		audioSource.SetScheduledEndTime(time);
		}
	}

	/// Changes the time at which a sound that has already been scheduled to play will start.
	public void SetScheduledStartTime(double time)
	{
		if (audioSource != null)
		audioSource.SetScheduledStartTime(time);
	}

	public void Stop ()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
			ShutdownSource();
			isPaused = true;
		}
	}

	public void UnPause ()
	{
		if (audioSource != null)
		{
			audioSource.UnPause();
			isPaused = false;
		}
	}

	bool InitializeSource () {
		if (Application.platform == RuntimePlatform.Android)
		{
			// TODO: VR_Audio bug on Android with Unity 2017
			return true;
		}
		if (id < 0)
		{
			id = VR_Audio.CreateAudioSource(hrtfEnabled);
			if (id >= 0)
			{
				VR_Audio.UpdateAudioSource(id, this, currentOcclusion);
				audioSource.spatialize = true;
				audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.Type,
												(float) VR_Audio.SpatializerType.Source);
				audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.Gain,
												VR_Audio.ConvertAmplitudeFromDb(gainDb));
				audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.MinDistance,
												sourceMinDistance);
				audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.ZeroOutput, 0.0f);
				// Source id must be set after all the spatializer parameters, to ensure that the source is
				// properly initialized before processing.
				audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.Id, (float) id);
			}
		}
		return id >= 0;
	}

	void ShutdownSource ()
	{
		if (id >= 0)
		{
			audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.Id, -1.0f);
			// Ensure that the output is zeroed after shutdown.
			audioSource.SetSpatializerFloat((int) VR_Audio.SpatializerData.ZeroOutput, 1.0f);
			audioSource.spatialize = false;
			VR_Audio.DestroyAudioSource(id);
			id = -1;
		}
	}

	void OnDidApplyAnimationProperties ()
	{
		OnValidate ();
	}

	void OnValidate ()
	{
		Clip = sourceClip;
		Loop = sourceLoop;
		mute = sourceMute;
		pitch = sourcePitch;
		priority = sourcePriority;
		spatialBlend = sourceSpatialBlend;
		volume = sourceVolume;
		dopplerLevel = sourceDopplerLevel;
		spread = sourceSpread;
		minDistance = sourceMinDistance;
		maxDistance = sourceMaxDistance;
		rolloffMode = sourceRolloffMode;
	}

	void OnDrawGizmosSelected ()
	{
		// Draw listener directivity gizmo.
		// Note that this is a very suboptimal way of finding the component, to be used in Unity Editor
		// only, should not be used to access the component in run time.
		GvrAudioListener listener = FindObjectOfType<GvrAudioListener>();
		if(listener != null) {
		Gizmos.color = VR_Audio.listenerDirectivityColor;
		DrawDirectivityGizmo(listener.transform, listenerDirectivityAlpha,
							listenerDirectivitySharpness, 180);
		}
		// Draw source directivity gizmo.
		Gizmos.color = VR_Audio.sourceDirectivityColor;
		DrawDirectivityGizmo(transform, directivityAlpha, directivitySharpness, 180);
	}

	void DrawDirectivityGizmo (Transform target, float alpha, float sharpness, int resolution)
	{
		Vector2[] points = VR_Audio.Generate2dPolarPattern(alpha, sharpness, resolution);
		int numVertices = resolution + 1;
		Vector3[] vertices = new Vector3[numVertices];
		vertices[0] = Vector3.zero;
		for (int i = 0; i < points.Length; ++i)
			vertices[i + 1] = new Vector3(points[i].x, 0.0f, points[i].y);
		int[] triangles = new int[6 * numVertices];
		for (int i = 0; i < numVertices - 1; ++i)
		{
			int index = 6 * i;
			if (i < numVertices - 2)
			{
				triangles[index] = 0;
				triangles[index + 1] = i + 1;
				triangles[index + 2] = i + 2;
			} 
			else
			{
				triangles[index] = 0;
				triangles[index + 1] = numVertices - 1;
				triangles[index + 2] = 1;
			}
			triangles[index + 3] = triangles[index];
			triangles[index + 4] = triangles[index + 2];
			triangles[index + 5] = triangles[index + 1];
		}
		Mesh directivityGizmoMesh = new Mesh();
		directivityGizmoMesh.hideFlags = HideFlags.DontSaveInEditor;
		directivityGizmoMesh.vertices = vertices;
		directivityGizmoMesh.triangles = triangles;
		directivityGizmoMesh.RecalculateNormals();
		Vector3 scale = 2.0f * Mathf.Max(target.lossyScale.x, target.lossyScale.z) * Vector3.one;
		Gizmos.DrawMesh(directivityGizmoMesh, target.position, target.rotation, scale);
	}
}
