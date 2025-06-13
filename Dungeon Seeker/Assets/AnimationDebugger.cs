using UnityEngine;

public class AnimationDebugger : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (anim == null)
        {
            Debug.LogError("No Animator component found!");
        }
    }
    
    void Update()
    {
        // Press F1 to reset all animations (useful for fixing stuck animations)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ResetAllAnimations();
            Debug.Log("All animations reset!");
        }
        
        // Press F2 to log animation states
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LogAnimationStates();
        }
    }
    
    void ResetAllAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("run", false);
            anim.SetBool("jump", false);
            anim.SetBool("fall", false);
            anim.SetBool("attack", false);
            anim.SetBool("wallSlide", false);
            anim.SetInteger("attackCombo", 0);
            anim.SetBool("grounded", true);
            
            // Reset animator
            anim.Rebind();
            anim.Update(0f);
        }
    }
    
    void LogAnimationStates()
    {
        if (anim != null)
        {
            Debug.Log("Animation States:" +
                "\nrun: " + anim.GetBool("run") +
                "\njump: " + anim.GetBool("jump") +
                "\nfall: " + anim.GetBool("fall") +
                "\nattack: " + anim.GetBool("attack") +
                "\nwallSlide: " + anim.GetBool("wallSlide") +
                "\nattackCombo: " + anim.GetInteger("attackCombo") +
                "\ngrounded: " + anim.GetBool("grounded"));
        }
    }
}
