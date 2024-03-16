using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpriteColor : MonoBehaviour
{
    SpriteRenderer _spriteRenderer;
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
    }
}
