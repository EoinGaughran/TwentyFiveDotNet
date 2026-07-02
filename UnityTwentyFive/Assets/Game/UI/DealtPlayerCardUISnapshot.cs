using System.Collections.Generic;
using UnityEngine;

public class DealtPlayerCardUISnapshot
{
    public int PlayerId { get; set; }
    public List<CardUI> CardUIs { get; set; }
    public List<CardUI> CardAnimUIs { get; set; }
}