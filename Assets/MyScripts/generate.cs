using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generate : MonoBehaviour
{
    public Texture2D image;
    public Texture2D heightMap;
    Renderer m_Renderer;

    void Start()
    {
        Pixelreader();
    }

    void Pixelreader()
    {
        // Iterate through it's pixels
        for (int i = 0; i < image.width; i++)
        {
            for (int j = 0; j < image.height; j++)
            {
                Color pixel = image.GetPixel(i, j);

                Debug.Log(pixel +" "+ i + " " + j);

                CreateStuff(i, j);
            }
        }
    }
    void CreateStuff(int i, int j)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_Renderer = p.GetComponent<Renderer>();
        m_Renderer.material.color = image.GetPixel(i, j);
        float pos = heightMap.GetPixel(i, j).grayscale; 
        Debug.Log(pos);
        p.transform.position = new Vector3(i , pos, j);
    }
}
    
