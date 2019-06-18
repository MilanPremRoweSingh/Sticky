using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PipeExit : MonoBehaviour
{

    public Animator animator;

    [SerializeField]
    public string scenePath;

    public PlayerController player;

    private AsyncOperation asyncLoad;

    private bool playerInTrigger = false;
    private bool enteringPipe = false;
    private bool fadeOutComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        asyncLoad = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.attemptingPipeEnter && playerInTrigger && asyncLoad != null)
        {
            enteringPipe = true;
            player.MoveToNoPhysics(player.Pos2D() + Vector2.down, 1.0f);

            fadeOutComplete = false;
            animator.SetTrigger("FadeOut");
        }

        if (fadeOutComplete && enteringPipe && player.rpp.enabled && asyncLoad != null)
        {
            ActivateLoadedScene();
        }
    }

    public void OnFadeOutComplete()
    {
        fadeOutComplete = true;
    }

    void ActivateLoadedScene()
    {
        asyncLoad.allowSceneActivation = true;
    }

    public void EnterNextScene()
    { }

    IEnumerator LoadNextScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync(scenePath);
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            if (player != null && asyncLoad == null)
            {
                StartCoroutine(LoadNextScene());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
}
