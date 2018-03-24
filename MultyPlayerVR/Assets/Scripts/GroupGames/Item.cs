using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public int Id;
    public string Title { get; set; }
    public float Power { get; set; }
    public float Health { get; set; }
    public string Description { get; set; }
    public string iconName;
    public string prefabName { get; set; }
    public Sprite Sprite { get; set; }
    public int Stackable;
    public int ReceiveItemId { get; set; }

    public Item()
    {
        this.Id = -1;
    }
    public void SetData(Item data)
    {
        this.Id = (int)data.Id;
        this.Title = data.Title;
        this.Power = (int)data.Power;
        this.Description = data.Description;
        this.iconName = data.iconName;
        this.prefabName = data.prefabName;
        this.Sprite = data.Sprite;
        this.Stackable = data.Stackable;
        this.ReceiveItemId = data.ReceiveItemId;
    }
}
