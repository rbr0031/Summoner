using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LifeManaHandler : MonoBehaviour
{
    public Image lifeBar;
    public Image manaBar;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI manaText;

    public float myLife;
    public float myMana;

    private float currentLife;
    private float currentMana;
    private float calculateLife;

    void Start()
    {
        currentLife = myLife;
        currentMana = myMana;
    }


    void Update()
    {
        calculateLife = currentLife / myLife;
        lifeBar.fillAmount = Mathf.MoveTowards(lifeBar.fillAmount, calculateLife, Time.deltaTime);
        lifeText.text = "" + (int)currentLife;

        if (currentMana < myMana)
        {
            manaBar.fillAmount = Mathf.MoveTowards(manaBar.fillAmount, 1f, Time.deltaTime * 0.01f);
            currentMana = Mathf.MoveTowards(currentMana / myMana, 1f, Time.deltaTime * 0.01f) * myMana;
        }

        if (currentMana < 0)
        {
            currentMana = 0;
        }

        manaText.text = "" + Mathf.FloorToInt(currentMana);

        if (currentLife <= 0)
        {
            //dead
        }
    }

    public void Damage(float damage)
    {
        currentLife -= damage;
    }

    public void ReduceMana(float mana)
    {
        if (mana <= currentMana)
        {
            currentMana -= mana;
            manaBar.fillAmount -= mana / myMana;
        }
        else
        {
            //No mana!
        }

    }
}