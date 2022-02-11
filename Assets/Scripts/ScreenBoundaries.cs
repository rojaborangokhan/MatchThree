using UnityEngine;

namespace MyUtils
{
    public struct ScreenBoundaries
    {
        public static Vector2 GetScreenBoundaries(float xPosFactor, float xOffset, float yPosFactor, float yOffset)
        {
            Vector2 screenBoundaries = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));

            Vector2 position = new Vector2(xPosFactor * screenBoundaries.x + xOffset, yPosFactor * screenBoundaries.y + yOffset);
        
            return position;
        }
    }
}
