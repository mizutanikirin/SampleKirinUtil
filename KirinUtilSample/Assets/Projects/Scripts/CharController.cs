using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KirinUtil;
using UnityEngine.Events;

public class CharController : MonoBehaviour
{
    // Public Vars
    [Header("Params")]
    public float Interval;
    public float Speed;
    public int Score;

    [System.Serializable]
    public class CharHitEvent : UnityEvent<int> { }
    public CharHitEvent HitEvent = new CharHitEvent();

    // Private Vars
    private bool is_hit;
    private bool is_moving;

    // Start is called before the first frame update
    void Awake()
    {
        is_hit = false;
        is_moving = false;
    }

    public void StartMoving()
    {
        Debug.Log(gameObject.name + ":StartMoving()");

        is_moving = true;

        ShowCharacter();

        Util.media.MoveRandom(gameObject, Speed, Interval, 0);
    }

    public void StopMoving()
    {
        is_moving = false;

        Util.media.StopMoveRandom(gameObject);
    }

    public void ShowCharacter()
    {
        is_hit = false;

        MoveToRandomPos();

        Util.image.PlayImage(gameObject.name);
        Util.media.FadeInUI(gameObject, 0.25f, 0);
    }

    public void HideCharacter()
    {
        Util.media.FadeOutUI(gameObject, 1f, 0);
    }

    public void StopCharacter()
    {
        Util.image.StopImage(gameObject.name);
    }

    private void MoveToRandomPos()
    {
        transform.localPosition = new Vector3(Random.Range(-800f, 800f), Random.Range(-440f, 440f), 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit with Hands
        if (is_moving && !is_hit)
        {
            is_hit = true;

            HitEvent.Invoke(Score);

            StartCoroutine(HitAction());
        }
    }

    IEnumerator HitAction()
    {
        Util.sound.PlaySE(0);

        Util.media.StopMoveRandom(gameObject);
        Util.media.MoveScale(gameObject, 1.35f, 0.03f, 0, 6, true);

        yield return new WaitForSeconds(0.3f);

        Util.media.FadeOutUI(gameObject, 0.5f, 0);
    }
}
