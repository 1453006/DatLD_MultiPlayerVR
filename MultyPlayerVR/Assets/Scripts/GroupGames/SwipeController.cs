using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwipeAngle
{
    TOP = 0,
    RIGHT,
    DOWN,
    LEFT
}
public class SwipeController : MonoBehaviour {

    private bool firstTouch = true;
    private Vector2 touchOrigin;

    /// Swipe actions return the index of the selected menu item.
    public delegate void SwipeAction(SwipeAngle selectedIx);
    public event SwipeAction OnSwipeSelect;

    int numItems;
    // Use this for initialization
    void Start () {
       numItems = System.Enum.GetNames(typeof(SwipeAngle)).Length;
    }
	
	// Update is called once per frame
	void Update () {
        // Menu constants.
      
        float radiansPerItem = 2.0f * Mathf.PI / numItems;
        const float MIN_INNER_RADIUS = 0.02f;
        const float MAX_OUTER_RADIUS = 0.25f;
        const float SELECT_RADIUS = 0.04f;

        // Touchpad constants.
        bool touching = GvrControllerInput.IsTouching;
        Vector2 touchPos = GvrControllerInput.TouchPos;
        touchPos.y = -touchPos.y;

        // Set the origin of the touch.
        if (touching && firstTouch)
        {
            touchOrigin = touchPos;
            firstTouch = false;
        }
        else if (!touching)
        {
            firstTouch = true;
        }

        if (GvrControllerInput.ClickButton)
        {
            touchOrigin = touchPos;
        }

        // Get the touch delta from the origin.
        Vector2 p = (touchPos - touchOrigin) * MAX_OUTER_RADIUS;
        float pMag = p.magnitude;

        for (int i = 0; i < numItems; ++i)
        {
            float theta = radiansPerItem * i;
            Vector2 dir2D = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
            Vector3 dir = new Vector3(Mathf.Sin(theta), 0.0f, Mathf.Cos(theta));
           

            if (touching && pMag > MIN_INNER_RADIUS &&
                Mathf.Deg2Rad * Vector2.Angle(dir2D, p) < radiansPerItem * 0.5f)
            {
                float blendDist = Mathf.Min(1.0f, (pMag - MIN_INNER_RADIUS) / SELECT_RADIUS);
                if (pMag - MIN_INNER_RADIUS >= SELECT_RADIUS)
                {
                    if(OnSwipeSelect != null)
                        OnSwipeSelect.Invoke((SwipeAngle)i);
                }
            }
        }
    }

}
