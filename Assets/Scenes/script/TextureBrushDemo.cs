using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBrushDemo : MonoBehaviour
{

    Texture2D texture = null;
    // Start is called before the first frame update
    Brush brush = null;
    Vector2 screenSize ;
    void Start()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
        var camera = this.GetComponent<Camera>();
        camera.aspect = screenSize.x / screenSize.y;
        camera.orthographicSize = screenSize.y * 0.5f;
        var bg = this.transform.GetChild(0);
        bg.localScale = new Vector3(screenSize.x, screenSize.y, 1.0f);
        var bg_m = bg.GetComponent<MeshRenderer>().material;
        texture = new Texture2D(Screen.width, Screen.height);
        bg_m.mainTexture = texture;
        brush = new Brush(texture);
    }

    private Vector2 _touch = new Vector2(-1, -1);

    private Vector2 touch
    {
        get
        {
            return _touch;
        }
        set
        {
            if(_touch != value )
            {
                Vector2 old = _touch;
                _touch = value;

                if(old != new Vector2(-1, -1))
                    this.OnTouchPositionChanged(old, _touch);
            }
        }
    }

    private void OnTouchPositionChanged(Vector2 oldPosition, Vector2 newPosition)
    {
        float dis = Vector2.Distance(Vector2.zero, newPosition - oldPosition);

        if(dis > 0f)
        {
            for (float i = 0; i < dis; i += 10f)
            {
                brush.Draw(oldPosition + i * (newPosition - oldPosition).normalized);
            }

            brush.Apply();
        }
    }

    private bool gestureStarted = false;
    Vector3 position;

    Vector2 GetTouch()
    {
        Vector3 pos = Input.mousePosition;
        Vector2 position = new Vector2(Mathf.Max(0f, Mathf.Min(screenSize.x, pos.x)), Mathf.Max(0f, Mathf.Min(screenSize.y, pos.y)));
        return position;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Vector2 t = GetTouch();

            if(!gestureStarted)
            {
                _touch = t;;
                gestureStarted = true;
            }
            else
            {
                touch = t;
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(!gestureStarted) return;

            if(gestureStarted)
            {
                touch = GetTouch();
            }

            _touch = new Vector2(-1, -1);
            gestureStarted = false;
        }
    }
}
