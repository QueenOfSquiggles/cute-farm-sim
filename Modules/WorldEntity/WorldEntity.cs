using System;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(WorldEntity))]
public partial class WorldEntity : Resource
{

    [Export] public string ID = "generic_id";
    [Export] public bool CanCollect = true;
    [Export] public Texture InventoryIcon;
    [Export] public PackedScene WorldScene;

}