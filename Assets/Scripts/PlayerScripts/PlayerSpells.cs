using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSpells : MonoBehaviour
{
    public string spellName;
    public string spellDescription;
    public int manaCost;
    public float cooldownTime;
    public Texture icon;
    public GameObject castEffect;

    public abstract void CastSpell(Vector3 targetPosition);
}
