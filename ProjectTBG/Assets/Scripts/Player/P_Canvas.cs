using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class P_Canvas : MonoBehaviour
{
    // Variables //

    public GameObject inGamePanel;
    public GameObject inventoryPanel;

    public TextMeshProUGUI ammoText;
    public Slider healthSlider;

    private bool openInventory;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAmmoText(float ammo, float inClip)
    {
        ammoText.text = ammo + " | " + inClip;
    }

    public void UpdateHealthSlider(float health, float maxHealth)
    {
        healthSlider.value = health / maxHealth;
    }

    // Input Methods //

    public void ToggleInventory(InputAction.CallbackContext context)
    {
        openInventory = !openInventory;

        inventoryPanel.GetComponent<Animator>().SetBool("Open",openInventory);
    }
}
