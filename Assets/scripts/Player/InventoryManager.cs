using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private List<string> keys = new List<string>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddKey(string keyName)
    {
        keys.Add(keyName);
        Debug.Log("Llave añadida al inventario.");
    }

    public bool HasKey(string keyName)
    {
        return keys.Contains(keyName);
    }

    public void RemoveKey(string keyName)
    {
        if (keys.Contains(keyName))
        {
            keys.Remove(keyName);
            Debug.Log("Llave usada y eliminada del inventario.");
        }
    }
}
