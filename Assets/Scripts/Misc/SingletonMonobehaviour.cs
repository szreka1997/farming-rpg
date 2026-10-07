using UnityEngine;

// Abstract class - cannot be instantiated, but they can be subclassed
// Generics <T>   - like GetComponent<Player>();
public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    // Static field - can be called without instance
    private static T instance;

    public static T Instance { 
        get {
            return instance;
        }
    }


    // Virtual method - allow subclasses of the type to override the method
    // Protected      - subclasses can access it
    protected virtual void Awake() {
        if(instance == null) {
            instance = this as T;
        } else {
            Destroy(gameObject);
        }
    }
        


    
}
