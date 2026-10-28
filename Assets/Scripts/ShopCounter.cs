using UnityEngine;

public class ShopCounter : MonoBehaviour
{
    bool playerInRange;

    void OnTriggerEnter2D(Collider2D o) { if (o.GetComponentInParent<PlayerController2D>()) playerInRange = true; }
    void OnTriggerExit2D(Collider2D o)  { if (o.GetComponentInParent<PlayerController2D>()) playerInRange = false; }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            Debug.Log("Ouverture de la boutique !");
    }
}