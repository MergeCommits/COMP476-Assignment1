using UnityEngine;

public class Flag : MonoBehaviour {
    private Vector3 origin;
    
    void Start() {
        origin = transform.position;
    }

    public void FlagReset() {
        Transform transform1 = transform;
        transform1.parent = null;
        transform1.position = origin;
    }
}
