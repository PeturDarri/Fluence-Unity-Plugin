using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class FluencePlugin : MonoBehaviour
{
	public static FluencePlugin Instance;

	[Range(0f, 1f)]
	public float StencilFadeValue = 1f;

	public FluenceLightfield[] Lightfields;
	
	private Camera[] _cameras;

	private float[] _eyeFromStartMatrixRaw;

	private float[] _clipFromEyeMatrixRaw;

	private float[] _startFromLightfieldMatrixRaw;

	private int[] _viewportRaw;

	private Matrix4x4 _flipMatrix;

	private void Awake()
	{
		Instance = this;
		_cameras = FindObjectsOfType<Camera>();
		for (int i = 0; i < _cameras.Length; i++)
		{
			FluenceCamera fluenceCamera = _cameras[i].gameObject.AddComponent<FluenceCamera>();
			fluenceCamera.CameraIndex = i;
		}
		SetupPlugin();
		SetEyeCount(_cameras.Length);
		_eyeFromStartMatrixRaw = new float[16];
		_clipFromEyeMatrixRaw = new float[16];
		_startFromLightfieldMatrixRaw = new float[16];
		_viewportRaw = new int[4];
		_flipMatrix = Matrix4x4.identity;
		_flipMatrix[2, 2] = -1f;
		foreach (FluenceLightfield fluenceLightfield in Lightfields)
		{
			string text = Path.Combine(Application.streamingAssetsPath, fluenceLightfield.LightfieldPath);
			if (File.Exists(text))
			{
				fluenceLightfield.LightfieldIndex = LoadLightfield(text);
			}
			else
			{
				Debug.LogError("Could not find file: " + text);
				fluenceLightfield.LightfieldIndex = -1;
			}
		}
		Init();
		GL.IssuePluginEvent(GetRenderEventFunc(), 1179406079);
	}

	public void StartFluence()
	{
		GL.IssuePluginEvent(GetRenderEventFunc(), 1179406077);
		for (int i = 0; i < _cameras.Length; i++)
		{
			CommandBuffer commandBuffer = new CommandBuffer {name = "Fluence Render"};
			commandBuffer.IssuePluginEvent(GetRenderEventFunc(), 1179405824 | i);
			commandBuffer.IssuePluginEvent(GetRenderEventFunc(), 1179406078);
			commandBuffer.IssuePluginEvent(GetRenderEventFunc(), 1179406077);
			_cameras[i].AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer);
		}
	}

	private void OnDisable()
	{
		GL.IssuePluginEvent(GetRenderEventFunc(), 1179406076);
	}

	public void SetRenderEyePublic(int cameraIndex)
	{
		FloatArrayFromMatrix4X4(_cameras[cameraIndex].worldToCameraMatrix, ref _eyeFromStartMatrixRaw);
		FloatArrayFromMatrix4X4(_cameras[cameraIndex].projectionMatrix, ref _clipFromEyeMatrixRaw);
		IntArrayFromRect(_cameras[cameraIndex].pixelRect, ref _viewportRaw);
		UpdateEye(cameraIndex, _eyeFromStartMatrixRaw, _clipFromEyeMatrixRaw, _viewportRaw);
	}

	public static void PrepareLightfieldPublic(int lightfieldIndex)
	{
		PrepareLightfield(lightfieldIndex);
	}

	public void PrepareLightfieldParallelPublic(int lightfieldIndex)
	{
		PrepareLightfieldParallel(lightfieldIndex);
	}

	public void UnprepareLightfieldPublic(int lightfieldIndex)
	{
		UnprepareLightfield(lightfieldIndex);
	}

	public static bool IsLightfieldReadyPublic(int lightfieldIndex)
	{
		return IsLightfieldReady(lightfieldIndex);
	}

	private void UpdateLightfieldsAndEyes()
	{
		SetStencilFadeValue(StencilFadeValue);
		foreach (FluenceLightfield fluenceLightfield in Lightfields)
		{
			if (fluenceLightfield.LightfieldIndex >= 0)
			{
				SetFadeValue(fluenceLightfield.LightfieldIndex, fluenceLightfield.FadeValue);
				SetRenderOrder(fluenceLightfield.LightfieldIndex, fluenceLightfield.RenderOrder);
				FloatArrayFromMatrix4X4(fluenceLightfield.transform.localToWorldMatrix * _flipMatrix, ref _startFromLightfieldMatrixRaw);
				UpdateStartFromLightfieldMatrix(fluenceLightfield.LightfieldIndex, _startFromLightfieldMatrixRaw);
			}
		}
	}

	private void Update()
	{
		UpdateLightfieldsAndEyes();
	}

	private static void IntArrayFromRect(Rect rect, ref int[] rectRaw)
	{
		rectRaw[0] = (int)rect.xMin;
		rectRaw[1] = (int)rect.yMin;
		rectRaw[2] = (int)rect.xMax;
		rectRaw[3] = (int)rect.yMax;
	}

	private static void FloatArrayFromMatrix4X4(Matrix4x4 matrix, ref float[] matrixRaw)
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int num = j * 4 + i;
				matrixRaw[num] = matrix[j, i];
			}
		}
	}

	[DllImport("fluence_plugin")]
	private static extern void SetupPlugin();

	[DllImport("fluence_plugin")]
	private static extern void Init();

	[DllImport("fluence_plugin")]
	private static extern void SetPaused(bool paused);

	[DllImport("fluence_plugin")]
	private static extern int LoadLightfield([MarshalAs(UnmanagedType.LPStr)] string filePath);

	[DllImport("fluence_plugin")]
	private static extern void PrepareLightfield(int lightfieldIndex);

	[DllImport("fluence_plugin")]
	private static extern void PrepareLightfieldParallel(int lightfieldIndex);

	[DllImport("fluence_plugin")]
	private static extern void UnprepareLightfield(int lightfieldIndex);

	[DllImport("fluence_plugin")]
	private static extern bool IsLightfieldReady(int lightfieldIndex);

	[DllImport("fluence_plugin")]
	private static extern void SetEyeCount(int eyeCount);

	[DllImport("fluence_plugin")]
	private static extern void SetStencilFadeValue(float value);

	[DllImport("fluence_plugin")]
	private static extern void SetFadeValue(int lightfieldIndex, float value);

	[DllImport("fluence_plugin")]
	private static extern void SetRenderOrder(int lightfieldIndex, float value);

	[DllImport("fluence_plugin")]
	private static extern void SetIsEnabled(int lightfieldIndex, bool value);

	[DllImport("fluence_plugin")]
	public static extern void SetRenderEye(int eyeIndex);

	[DllImport("fluence_plugin")]
	private static extern void SetUnityDebugOutputFunc(IntPtr fp);

	[DllImport("fluence_plugin")]
	private static extern void UpdateEye(int eyeIndex, float[] eyeFromStartMatrix, float[] clipFromEyeMatrix, int[] viewportBounds);

	[DllImport("fluence_plugin")]
	private static extern void UpdateStartFromLightfieldMatrix(int lightfieldIndex, float[] startFromLightfieldMatrix);

	[DllImport("fluence_plugin")]
	private static extern IntPtr GetRenderEventFunc();
}