using System.Collections;
using UnityEngine;

//An example of how to load a lightfield
public class ExampleFluenceController : MonoBehaviour
{
	public int LightfieldIndex;
	
	private void Start()
	{
		FluencePlugin.Instance.StartFluence();
		StartCoroutine(PrepareLightfieldRoutine(LightfieldIndex));
	}

	private IEnumerator PrepareLightfieldRoutine(int index)
	{
		Debug.Log("Preparing lightfield");
		FluencePlugin.PrepareLightfieldPublic(index);
		while (!FluencePlugin.IsLightfieldReadyPublic(index))
		{
			yield return 0;
		}
		Debug.Log("Lightfield ready!");
	}
}