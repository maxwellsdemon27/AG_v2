using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RF_PathPainter : MonoBehaviour
{
   

    public bool startsimulator = false;
    //public List<Vector2> vertices_2DRF = new List<Vector2>();

    public Vector2 std_point;
    //public Vector2[] stand_Vectors;

    public int mix_x = 0;
    public int max_x = 0;
    public int mix_y = 0;
    public int max_y = 0;

    public List<Vector2> rect_Points;

    public Vector2 pA;
    public Vector2 pB;
    public Vector2 pC;
    public Vector2 pD;

    //public List<Vector2> m_Points;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RF"))
        {
            if (startsimulator)
                GameObject.Instantiate(other.gameObject.transform.Find("RF_Canvas").Find("Rf_View").gameObject, this.transform,true);

            /*
            var RF_Pos = other.gameObject.transform.position;
            pA= new Vector2(RF_Pos.x, RF_Pos.z);
 
            RotatedPointGet(other.transform.parent.localEulerAngles.y,40);
            StandTranslate();
            GetScanRect();
            
            for (int i = mix_x; i < max_x; i++)
            {
                for (int j = mix_y; j < max_y; j++)
                {
                    if (RF_IN_CHECK(new Vector2(i,j)))
                        temp_tex.SetPixel(i, j, new Color(1, 1, 1));                   
                }
            }
            temp_tex.Apply();*/
        }
    }

    private void RotatedPointGet(float R,float len)
    {
        float p1_x = (float)Math.Cos(Math.PI * ((90 - R - 30) / 180)) * len * 1000 + pA.x;
        float p1_z = (float)Math.Sin(Math.PI * ((90 - R - 30) / 180)) * len * 1000 + pA.y;
        pB = new Vector2(p1_x, p1_z);

        float p2_x = (float)Math.Cos(Math.PI * ((90 - R + 0) / 180)) * len * 1000 + pA.x;
        float p2_z = (float)Math.Sin(Math.PI * ((90 - R + 0) / 180)) * len * 1000 + pA.y;
        pC = new Vector2(p2_x, p2_z);

        float p3_x = (float)Math.Cos(Math.PI * ((90 - R + 30) / 180)) * len * 1000 + pA.x;
        float p3_z = (float)Math.Sin(Math.PI * ((90 - R + 30) / 180)) * len * 1000 + pA.y;
        pD = new Vector2(p3_x, p3_z);
    }

    private void StandTranslate()
    {
        pA = new Vector2((int)((pA.x * (-1) + 60000) * 4096 / 120000), (int)((pA.y * (-1) + 60000) * 4096 / 120000));
        pB = new Vector2((int)((pB.x * (-1) + 60000) * 4096 / 120000), (int)((pB.y * (-1) + 60000) * 4096 / 120000));
        pC = new Vector2((int)((pC.x * (-1) + 60000) * 4096 / 120000), (int)((pC.y * (-1) + 60000) * 4096 / 120000));
        pD = new Vector2((int)((pD.x * (-1) + 60000) * 4096 / 120000), (int)((pD.y * (-1) + 60000) * 4096 / 120000));      
    }

    private void GetScanRect()
    {
        mix_x = (int)pA.x;
        max_x = (int)pA.x;
        mix_y = (int)pA.y;
        max_y = (int)pA.y;

        if (pB.x < mix_x)
            mix_x = (int)pB.x;
        if (pB.x > max_x)
            max_x = (int)pB.x;
        if (pB.y < mix_y)
            mix_y = (int)pB.y;
        if (pB.y > max_y)
            max_y = (int)pB.y;

        if (pC.x < mix_x)
            mix_x = (int)pC.x;
        if (pC.x > max_x)
            max_x = (int)pC.x;
        if (pC.y < mix_y)
            mix_y = (int)pC.y;
        if (pC.y > max_y)
            max_y = (int)pC.y;

        if (pD.x < mix_x)
            mix_x = (int)pD.x;
        if (pD.x > max_x)
            max_x = (int)pD.x;
        if (pD.y < mix_y)
            mix_y = (int)pD.y;
        if (pD.y > max_y)
            max_y = (int)pD.y;
    }

    private bool RF_IN_CHECK(Vector2 tex_point)
    {
        var cross_count = 0;
        var farPoint = new Vector2(5000, 5000);

        cross_count += CheckCross(farPoint, tex_point, pA, pB);
        cross_count += CheckCross(farPoint, tex_point, pB, pC);
        cross_count += CheckCross(farPoint, tex_point, pC, pD);
        cross_count += CheckCross(farPoint, tex_point, pD, pA);

        if (cross_count % 2 == 0)
            return false;
        else
            return true;
    }

    private int CheckCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        

        var AB_a = (a.y - b.y) / (a.x - b.x);
        var CD_a = (c.y - d.y) / (c.x - d.x);
        var AB_b = a.y - a.x * AB_a;
        var CD_b = c.y - c.x * CD_a;

        if (AB_a == CD_a)
            return 0;
        else
        {
            var cross_x = (CD_b - AB_b) / (AB_a - CD_a);

            if (
                (GetMin(a, b, true) < cross_x) &&
                (cross_x < GetMax(a, b, true)) &&
                (GetMin(c, d, true) < cross_x) &&
                (cross_x < GetMax(c, d, true))
                )
                return 1;
            else
                return 0;
            
        }
    }

    private float CrossProduct(Vector2 p1,Vector2 p2, Vector2 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p3.x - p1.x) * (p2.y - p1.y);
    }

    private int GetMax(Vector2 a, Vector2 b,bool get_x)
    {
        if(get_x)
        {
            if (a.x >= b.x)
                return (int)a.x;
            else
                return (int)b.x;
        }  
        else
        {
            if (a.y >= b.y)
                return (int)a.y;
            else
                return (int)b.y;
        }  
    }

    private int GetMin(Vector2 a, Vector2 b, bool get_x)
    {
        if (get_x)
        {
            if (a.x <= b.x)
                return (int)a.x;
            else
                return (int)b.x;
        }
        else
        {
            if (a.y <= b.y)
                return (int)a.y;
            else
                return (int)b.y;
        }
    }

    private void Awake()
    {
        /*
        material.SetTexture("_MainTex", ori_t);
        PainterInit();*/
    }

   
    // Start is called before the first frame update
    void Start()
    {
       

        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (startsimulator)
        {
            material.SetTexture("_MainTex", temp_tex);
        }
        else
        {
            material.SetTexture("_MainTex", ori_t);
        }*/
    }
}
