using UnityEngine;
using System.Collections;

#if USING_GETREAL3D
public class CAVE2RPCManager : getReal3D.MonoBehaviourWithRpc {

	public void DestroyByName( string name )
	{
		getReal3D.RpcManager.call("DestroyByNameRPC", name);
	}

	[getReal3D.RPC]
	void DestroyByNameRPC( string name )
	{
		Destroy(GameObject.Find(name));
	}

	public void CAVE2RPCMessage(params object[] param)
	{
		int i = 0;
		foreach(object p in param)
		{
			Debug.Log("["+i+"] "+p);
			i++;
		}
		GameObject obj = GameObject.Find((string)param[0]);
		if( obj != null )
		{
			getReal3D.RpcManager.call("BroadcastMessageRPC", param[1], param);
		}
		
	}
	
	[getReal3D.RPC]
	void BroadcastMessageRPC(string targetObjName, params object[] param)
	{
		GameObject obj = GameObject.Find((string)param[0]);
		if( obj != null )
		{
			obj.BroadcastMessage((string)param[1], param[2]);
		}
	}

}
#endif