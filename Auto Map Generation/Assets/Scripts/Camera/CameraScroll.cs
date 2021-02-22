using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    [SerializeField] private float keyScrollSpeed = 0.1f;

    private Transform tr;
    private Transform Tr
    {
        get
        {
            if(tr == null)
            {
                tr = transform;
            }
            return tr;
        }
    }

    private void Update ()
    {
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            Tr.position += new Vector3(0, keyScrollSpeed, 0);
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            Tr.position += new Vector3(0, -keyScrollSpeed, 0);
        }
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            Tr.position += new Vector3(-keyScrollSpeed, 0, 0);
        }
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            Tr.position += new Vector3(keyScrollSpeed, 0, 0);
        }
        return;
    }

}
