using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using EternityEngine;

public static class AudioUtilities
{
	const int HEADER_SIZE = 44;

	public static bool SaveWavFile (string fileName, AudioClip clip)
    {
		try
		{
			if (!fileName.ToLower().EndsWith(".wav"))
				fileName += ".wav";
			string filePath = Path.Combine(Application.dataPath, fileName);
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			using (FileStream fileStream = CreateEmpty(filePath))
			{
				ConvertAndWrite (fileStream, clip);
				WriteHeader (fileStream, clip);
			}
		}
		catch (Exception e)
		{
			return false;
		}
		return true;
	}

	public static AudioClip TrimSilence (AudioClip clip, float min)
    {
		float[] samples = new float[clip.samples];
		clip.GetData(samples, 0);
		return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
	}

	public static AudioClip TrimSilence (List<float> samples, float min, int channels, int hz)
    {
		return TrimSilence(samples, min, channels, hz, false, false);
	}

	public static AudioClip TrimSilence (List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
    {
		int i;
		for (i = 0; i < samples.Count; i ++)
        {
			if (Mathf.Abs(samples[i]) > min)
				break;
		}
		samples.RemoveRange(0, i);
		for (i = samples.Count - 1; i > 0; i --)
        {
			if (Mathf.Abs(samples[i]) > min)
				break;
		}
		samples.RemoveRange(i, samples.Count - i);
		AudioClip clip = AudioClip.Create("TempClip", samples.Count, channels, hz, _3D, stream);
		clip.SetData(samples.ToArray(), 0);
		return clip;
	}

	static FileStream CreateEmpty (string filepath)
    {
		FileStream fileStream = new FileStream(filepath, FileMode.Create);
	    byte emptyByte = new byte();
	    for (int i = 0; i < HEADER_SIZE; i ++)
	        fileStream.WriteByte(emptyByte);
		return fileStream;
	}

	static void ConvertAndWrite (FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples];
		clip.GetData(samples, 0);
		Int16[] intData = new Int16[samples.Length];
		Byte[] bytesData = new Byte[samples.Length * 2];
		int rescaleFactor = 32767;
		for (int i = 0; i < samples.Length; i ++)
        {
			intData[i] = (short) (samples[i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}
		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	static void WriteHeader (FileStream fileStream, AudioClip clip)
    {
		int hz = clip.frequency;
		int channels = clip.channels;
		int samples = clip.samples;
		fileStream.Seek(0, SeekOrigin.Begin);
		Byte[] riff = Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);
		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);
		Byte[] wave = Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);
		Byte[] fmt = Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);
		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);
		UInt16 two = 2;
		UInt16 one = 1;
		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);
		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);
		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);
		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
		fileStream.Write(byteRate, 0, 4);
		UInt16 blockAlign = (ushort) (channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);
		Byte[] datastring = Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);
		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);
	}
	
	public static AudioClip GetAudioClipFromWavFile (string fileName)
	{
		try
		{
			using (FileStream fs = File.Open(fileName, FileMode.Open))
			{
				BinaryReader reader = new BinaryReader(fs);
				int chunkID = reader.ReadInt32();
				int fileSize = reader.ReadInt32();
				int riffType = reader.ReadInt32();
				int fmtID = reader.ReadInt32();
				int fmtSize = reader.ReadInt32();
				int fmtCode = reader.ReadInt16();
				int channels = reader.ReadInt16();
				int sampleRate = reader.ReadInt32();
				int byteRate = reader.ReadInt32();
				int fmtBlockAlign = reader.ReadInt16();
				int bitDepth = reader.ReadInt16();
				if (fmtSize == 18)
				{
					int fmtExtraSize = reader.ReadInt16();
					reader.ReadBytes(fmtExtraSize);
				}
				int dataID = reader.ReadInt32();
				int bytes = reader.ReadInt32();
				byte[] byteArray = reader.ReadBytes(bytes);
				int bytesForSamp = bitDepth / 8;
				int nValues = bytes / bytesForSamp;
				float[] asFloat = null;
				switch (bitDepth)
				{
					case 64:
						double[] asDouble = new double[nValues];  
						Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
						asFloat = Array.ConvertAll(asDouble, e => (float) e);
						break;
					case 32:
						asFloat = new float[nValues];   
						Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
						break;
					case 16:
						Int16 [] asInt16 = new Int16[nValues];   
						Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
						asFloat = Array.ConvertAll(asInt16, e => e / (float) (Int16.MaxValue + 1));
						break;
					default:
						return null;
				}
				AudioClip output = AudioClip.Create(fileName + "(Generated)", nValues, channels, sampleRate, false);
				output.SetData(asFloat, 0);
				return output;
			}
		}
		catch
		{
			GameManager.Log ("Failed to load " + fileName);
			return null;
		}
		return null;
	}

	[Serializable]
	public struct AudioClipData
	{
		public float[] samples;
		public int channels;
		public int hz;
		public bool _3D;
		public bool stream;

		public AudioClipData (AudioClip audioClip)
		{
			channels = audioClip.channels;
			samples = new float[audioClip.samples * channels];
			audioClip.GetData(samples, 0);
			hz = audioClip.frequency;
			_3D = audioClip.ambisonic;
			stream = audioClip.loadType == AudioClipLoadType.Streaming;
		}

		public AudioClip ToAudioClip ()
		{
			AudioClip clip = AudioClip.Create("AudioClip (Generated)", samples.Length, channels, hz, _3D, stream);
			clip.SetData(samples, 0);
			return clip;
		}
	}
}