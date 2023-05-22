using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rectangles that the player can traverse over.
/// </summary>
public class Terrain : MonoBehaviour
{
    private BoxCollider2D box;
    private List<BoxCollider2D> cldrs = new List<BoxCollider2D>();

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        SplitCollider();
    }

    /// <summary>
    /// Split the box collider into 4 thin box colliders (1 for each side).
    /// </summary>
    private void SplitCollider()
    {
        for (int i = 0; i < 5; ++i)
        {
            GameObject child = new GameObject();
            child.transform.parent = transform;
            child.layer = gameObject.layer;
            child.tag = gameObject.tag;
            cldrs.Add(child.AddComponent<BoxCollider2D>());
        }
        
        PositionSplitCollider(cldrs[0], 0);     // top
        PositionSplitCollider(cldrs[1], 180);   // bottom
        PositionSplitCollider(cldrs[2], 90);    // left
        PositionSplitCollider(cldrs[3], -90);   // right
        PositionCenterCollider(cldrs[4]);

        box.enabled = false;
    }

    /// <summary>
    /// Position the center collider to be consistent with the original collider.
    /// </summary>
    private void PositionCenterCollider(BoxCollider2D cldr)
    {
        cldr.name = "Center";

        cldr.size = new Vector2(
            box.size.x - 0.006f,
            box.size.y - 0.006f
            );
        cldr.transform.localScale = Vector2.one;
        cldr.transform.localEulerAngles = Vector2.zero;
        cldr.transform.position = transform.position;
    }

    /// <summary>
    /// Position the side collider to be consistent with the original collider.
    /// </summary>
    private void PositionSplitCollider(BoxCollider2D cldr, float angle)
    {
        cldr.name = string.Format("Side {0}", angle);

        // Set size to match corresponding side of the original
        float len = (angle % 180 == 0) ? box.size.x : box.size.y;

        float invScaleX = Mathf.Abs(cldr.transform.localScale.x);
        float invScaleY = Mathf.Abs(cldr.transform.localScale.y);
        if (invScaleX > 1) invScaleX = 1;
        if (invScaleY > 1) invScaleY = 1;

        cldr.size = new Vector2(
            len / invScaleX, 
            0.003f / invScaleY
            );

        // Rotate to align with original
        cldr.transform.localEulerAngles = new Vector3(0, 0, angle);

        // Set position to align with original
        float lineThickness = cldr.size.y * cldr.transform.localScale.y;
        float degToRad = (angle + 90) * Mathf.PI / 180;

        float offsetX = (box.size.x - lineThickness) / 2 * Mathf.Cos(degToRad);
        float offsetY = (box.size.y - lineThickness) / 2 * Mathf.Sin(degToRad);

        cldr.transform.localPosition = new Vector3(
            box.offset.x + offsetX,
            box.offset.y + offsetY
            );
    }
}
