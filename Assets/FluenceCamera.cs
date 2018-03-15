using UnityEngine;

public class FluenceCamera : MonoBehaviour
{
 	public int CameraIndex;
	
	private void OnPreCull()
	{
		FluencePlugin.Instance.SetRenderEyePublic(CameraIndex);
	}
}