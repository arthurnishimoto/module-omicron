using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;
using System.Collections.Generic;

public class CanvasTouch : OmicronEventClient {

    public GameObject touchPointPrefab;

    Hashtable touchPoints;
    Dictionary<uint, string> touchGestureFlagNames;

	// Use this for initialization
	new void Start () {
        InitOmicron();
        touchPoints = new Hashtable();

        uint userFlag = 18;
        touchGestureFlagNames = new Dictionary<uint, string>
        {
            {userFlag << 2, "FLAG_SINGLE_TOUCH"},
            {userFlag << 3, "FLAG_BIG_TOUCH"},
            {userFlag << 4, "FLAG_FIVE_FINGER_HOLD"},
            {userFlag << 5, "FLAG_FIVE_FINGER_SWIPE"},
            {userFlag << 6, "FLAG_THREE_FINGER_HOLD"},
            {userFlag << 7, "FLAG_SINGLE_CLICK"},
            {userFlag << 8, "FLAG_DOUBLE_CLICK"},
            {userFlag << 9, "FLAG_MULTI_TOUCH"}
        };
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnEvent(EventData e)
    {
        if (e.serviceType == EventBase.ServiceType.ServiceTypePointer)
        {
            int id = (int)e.sourceId;
            Vector2 pos = new Vector2(e.posx, e.posy);
            EventBase.Type eventType = (EventBase.Type)e.type;
            EventBase.Flags flags = (EventBase.Flags)e.flags;

            pos = TouchPosToScreenPos(pos);

            int touchListSize = (int)e.getExtraDataFloat(4);

            if (eventType == EventBase.Type.Down)
            {
                AddTouchpoint(pos, id);
            }
            else if (eventType == EventBase.Type.Move)
            {
                if (touchPoints.ContainsKey(id))
                {
                    TouchPoint tp = (TouchPoint)touchPoints[id];
                    GameObject touchObject = ((TouchPoint)touchPoints[id]).GetGameObject();

                    string text = id.ToString();

                    touchObject.GetComponentInChildren<UnityEngine.UI.Text>().text = text;
                    touchObject.transform.localPosition = pos;

                    for (int i = 0; i < touchListSize; i++)
                    {
                        int sub_id = (int)e.getExtraDataFloat(5 + i * 3);
                        Vector2 sub_pos = new Vector2(e.getExtraDataFloat(6 + i * 3), e.getExtraDataFloat(7 + i * 3));
                        sub_pos = TouchPosToScreenPos(sub_pos);
                        if (touchPoints.ContainsKey(sub_id))
                        {
                            TouchPoint sub_tp = (TouchPoint)touchPoints[sub_id];
                            GameObject sub_touchObject = ((TouchPoint)touchPoints[sub_id]).GetGameObject();

                            string sub_text = sub_id.ToString();
                            sub_text += " (" + sub_tp.GetRootID() + ")";

                            sub_touchObject.GetComponentInChildren<UnityEngine.UI.Text>().text = sub_text;
                            sub_touchObject.transform.localPosition = sub_pos;
                        }
                        else
                        {
                            AddTouchpoint(sub_pos, sub_id, id);
                        }
                    }
                }
            }
            else if (eventType == EventBase.Type.Up)
            {
                Debug.Log("Up event for ID: " + id);
                Debug.Log(touchPoints.ContainsKey(id));
                if (touchPoints.ContainsKey(id))
                {
                    GameObject touchObject = ((TouchPoint)touchPoints[id]).GetGameObject();
                    Destroy(touchObject);
                    touchPoints.Remove(id);

                    for (int i = 0; i < touchListSize; i++)
                    {
                        int sub_id = (int)e.getExtraDataFloat(5 + i * 3);
                        if (touchPoints.ContainsKey(sub_id))
                        {
                            GameObject sub_touchObject = ((TouchPoint)touchPoints[sub_id]).GetGameObject();
                            Destroy(sub_touchObject);
                            touchPoints.Remove(sub_id);
                        }
                    }
                }
            }
            
        }
    }

    Vector2 TouchPosToScreenPos(Vector2 touchPos)
    {
        touchPos.x *= Screen.width;
        touchPos.y *= Screen.height;

        touchPos.x -= Screen.width / 2;
        touchPos.y -= Screen.height / 2;

        touchPos.y = -touchPos.y;

        return touchPos;
    }

    void AddTouchpoint(Vector2 pos, int id)
    {
        AddTouchpoint(pos, id, id);
    }

    void AddTouchpoint(Vector2 pos, int id, int root)
    {
        if (!touchPoints.ContainsKey(id))
        {
            TouchPoint touchPoint = new TouchPoint(pos, id);

            GameObject touchObject = Instantiate(touchPointPrefab);
            touchObject.name = id.ToString();
            touchObject.transform.SetParent(transform);

            touchObject.transform.localPosition = pos;
            string text = id.ToString();

            touchObject.GetComponentInChildren<UnityEngine.UI.Text>().text = text;

            touchPoint.SetGameObject(touchObject);
            touchPoint.SetRootID(root);
            touchPoints.Add(id, touchPoint);
        }

    }
}
