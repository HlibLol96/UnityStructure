using System.Collections.Generic;

public class CharackterModel
{
    public float MoveSpeed { get; set; }
    public int Health { get; set; }
    public int Strength { get; set; }
    public int MaxHealth { get; set; }
    public List<InvenbtoryItem> Inventory { get; set; } = new List<InvenbtoryItem>();
    public CharackterModel(float moveSpeed, int health, int strength, int maxHealth)
    {
        MoveSpeed = moveSpeed;
        Health = health;
        Strength = strength;
        MaxHealth = maxHealth;
    }
    public CharackterModel() { }
}
