using UnityEngine;

public class IsometricSorting : MonoBehaviour
{
    Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        int order = GameConstants.ISOMETRIC_SORT_BASE_ORDER - Mathf.RoundToInt(transform.position.y * GameConstants.ISOMETRIC_SORT_SCALE);
        if (_renderer != null)
            _renderer.sortingOrder = order;

        for (int i = 0; i < transform.childCount; i++)
        {
            var r = transform.GetChild(i).GetComponent<Renderer>();
            if (r != null)
                r.sortingOrder = order + 1;
        }
    }
}
